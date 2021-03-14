using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Shell;
using DevExpress.Mvvm;
using Setup.Model;
using Setup.Resources;

namespace Setup.ViewModel
{
    public class MainViewModel
    {
        #region Properties

        public ICollectionView Pages { get; private set; }

        public long ArchiveSize { get; private set; }

        public long SpaceRequired { get; private set; }

        public virtual long SpaceAvailable { get; protected set; }

        public virtual long ExtractionProgress { get; protected set; }

        public virtual string SetupLocation { get; set; }

        public virtual string DefaultSetupLocation { get; set; }

        public virtual string ProductName { get; set; }

        public virtual string ApplicationTitle { get; set; }

        public virtual string ApplicationDescription { get; set; }

        public virtual bool CreateShortcut { get; set; }

        public virtual bool LaunchApplication { get; set; }

        public virtual bool IsLicenseAccepted { get; set; }

        #endregion

        #region Services

        public virtual IModernDialogService ModernDialogService { get { return null; } }

        public virtual ITaskbarButtonService TaskbarButtonService { get { return null; } }

        public virtual ICurrentWindowService CurrentWindowService { get { return null; } }

        public virtual IFolderBrowserDialogService FolderBrowserDialogService { get { return null; } }

        #endregion

        #region Fields

        private WizardLink[] Links;

        private const int MB = 1048576;

        private Dictionary<string, FileSystemInfo> FileSystemEntries = new Dictionary<string, FileSystemInfo>(StringComparer.OrdinalIgnoreCase);

        #endregion

        public MainViewModel()
        {
            Links = new WizardLink[]
            {
                new WizardLink { Source = new Uri("/Pages/WelcomePage.xaml", UriKind.Relative), AllowBack=false },
                new WizardLink { Source = new Uri("/Pages/LicensePage.xaml", UriKind.Relative) },
                new WizardLink { Source = new Uri("/Pages/SettingsPage.xaml", UriKind.Relative) },
                new WizardLink { Source = new Uri("/Pages/SummaryPage.xaml", UriKind.Relative) },
                new WizardLink { Source = new Uri("/Pages/CompletionPage.xaml", UriKind.Relative), AllowBack=false, AllowNext=false, AllowCancel=false, AllowFinish=true}
            };

            Pages = CollectionViewSource.GetDefaultView(Links);

            ProductName = App.ExecutingAssembly.GetCustomAttribute<AssemblyProductAttribute>().Product;
            ApplicationTitle = App.ExecutingAssembly.GetCustomAttribute<AssemblyTitleAttribute>().Title;
            ApplicationDescription = App.ExecutingAssembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description;

            App.Current.MainWindow.Title = ApplicationTitle;
            DefaultSetupLocation = @"C:\" + ApplicationTitle;
        }

        public void SelectLocation()
        {
            FolderBrowserDialogService.StartPath = Directory.Exists(SetupLocation) ? SetupLocation : Path.GetDirectoryName(SetupLocation);
            if (FolderBrowserDialogService.ShowDialog())
            {
                if (FolderBrowserDialogService.ResultPath.EndsWith(ApplicationTitle))
                    SetupLocation = FolderBrowserDialogService.ResultPath;
                else
                    SetupLocation = Path.Combine(FolderBrowserDialogService.ResultPath, ApplicationTitle);
            }
        }

        public bool CanGoBack()
        {
            return true;
        }

        public void GoBack()
        {
            Pages.MoveCurrentToPrevious();
        }

        public bool CanGoNext()
        {
            switch (Pages.CurrentPosition)
            {
                case 1: return IsLicenseAccepted;
                case 2: return SetupLocation != null && SpaceAvailable > SpaceRequired;
            }

            return true;
        }

        public async Task GoNext()
        {
            if (Pages.CurrentPosition == 0)
            {
                string previousSetupLocation = Utilities.GetPreviousSetupLocation(ApplicationTitle);
                if (!String.IsNullOrEmpty(previousSetupLocation) && Directory.Exists(previousSetupLocation))
                    SetupLocation = previousSetupLocation;
                else
                    SetupLocation = DefaultSetupLocation;

                CreateShortcut = true;
                LaunchApplication = true;

                ArchiveSize = CalculateArchiveSize();
                SpaceRequired = ArchiveSize / MB;
            }
            else if (Pages.CurrentPosition == 3)
            {
                if (Utilities.CheckRunningApplication(ProductName))
                {
                    ModernDialogService.Show(String.Format(Properties.Resources.AlreadyRunning, ApplicationTitle), ApplicationTitle,
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    if (!Utilities.CloseRunningApplication(ProductName))
                    {
                        ModernDialogService.Show(String.Format(Properties.Resources.StillRunning, ApplicationTitle), ApplicationTitle,
                            MessageBoxButton.OK, MessageBoxImage.Error);

                        return;
                    }
                }

                await ExtractArchive();
                Cleanup();

                if (CreateShortcut)
                    Utilities.CreateShortcut(SetupLocation, ProductName, ApplicationTitle, ApplicationDescription);
            }

            Pages.MoveCurrentToNext();
        }

        public void Cancel()
        {
            if (ModernDialogService.Show(Properties.Resources.NotCompleted, ApplicationTitle,
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                CurrentWindowService.Close();
            }
        }

        public void Finish()
        {
            if (LaunchApplication)
            {
                try
                {
                    Process.Start(Path.Combine(SetupLocation, ProductName + ".exe"));
                }
                catch (Exception e)
                {
                    ModernDialogService.Show(e.Message, ApplicationTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            Utilities.SetCurrentSetupLocation(SetupLocation, ApplicationTitle);
            CurrentWindowService.Close();
        }

        protected void OnSetupLocationChanged()
        {
            DriveInfo driveInfo = new DriveInfo(Path.GetPathRoot(SetupLocation));
            SpaceAvailable = driveInfo.TotalFreeSpace / MB;
        }

        private long CalculateArchiveSize()
        {
            long archiveSize = 0;

            using (Stream stream = App.ExecutingAssembly.GetManifestResourceStream("Setup.Build.Package.zip"))
            {
                using (ZipArchive archive = new ZipArchive(stream))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                        archiveSize += entry.Length;
                }
            }

            return archiveSize;
        }

        private async Task ExtractArchive()
        {
            DirectoryInfo targetDirectory = new DirectoryInfo(SetupLocation);
            if (!targetDirectory.Exists)
                targetDirectory.Create();

            using (Stream stream = App.ExecutingAssembly.GetManifestResourceStream("Setup.Build.Package.zip"))
            {
                using (ZipArchive archive = new ZipArchive(stream))
                {
                    ExtractionProgress = 1;
                    TaskbarButtonService.ProgressState = TaskbarItemProgressState.Normal;

                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        await Task.Run(() =>
                        {
                            FileInfo fileInfo = new FileInfo(Path.Combine(SetupLocation, entry.FullName));
                            if (!fileInfo.Directory.Exists)
                                fileInfo.Directory.Create();

                            FileSystemEntries.Add(fileInfo.FullName, fileInfo);
                            if (!FileSystemEntries.ContainsKey(fileInfo.Directory.FullName))
                                FileSystemEntries.Add(fileInfo.Directory.FullName, fileInfo.Directory);

                            entry.ExtractToFile(fileInfo.FullName, true);
                        });

                        ExtractionProgress += entry.Length;
                        TaskbarButtonService.ProgressValue = (double)ExtractionProgress / ArchiveSize;
                    }

                    ExtractionProgress = ArchiveSize;
                    TaskbarButtonService.ProgressState = TaskbarItemProgressState.None;
                }
            }
        }

        private void Cleanup()
        {
            foreach (DirectoryInfo directory in FileSystemEntries.Values.OfType<DirectoryInfo>())
            {
                foreach (FileInfo file in directory.GetFiles())
                {
                    if (!FileSystemEntries.ContainsKey(file.FullName))
                        file.Delete();
                }
            }
        }
    }
}
