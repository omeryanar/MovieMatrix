using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shell;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Printing;
using DevExpress.XtraPrinting;
using MovieMatrix.Helper;
using MovieMatrix.Model;
using MovieStore;
using MovieStore.Container;

namespace MovieMatrix.ViewModel
{
    public abstract class BaseViewModel : ISupportParentViewModel, ISupportParameter
    {
        #region Services

        public virtual IMessageBoxService MessageBoxService { get { return null; } }

        public virtual ITaskbarButtonService TaskbarButtonService { get { return null; } }

        public virtual DevExpress.Mvvm.IDialogService DialogService { get { return null; } }

        public virtual ISaveFileDialogService SaveFileDialogService { get { return null; } }

        public virtual IOpenFileDialogService OpenFileDialogService { get { return null; } }

        public virtual ISelectFolderDialogService OpenFolderDialogService { get { return null; } }

        #endregion

        #region Properties

        public virtual object ParentViewModel { get; set; }

        public virtual object Parameter { get; set; }

        public dynamic Parameters { get; } = new ExpandoObject();

        #endregion

        #region Constants

        public const string ExcelFilter = "Excel Files |*.xls;*.xlsx;*.csv";

        public const string SubtitleFilter = "Subtitle Files |*.srt;*.sub;*.smi;*.txt;*.ssa;*.ass;*.mpl";

        public const string YoutubeVideoLink = "www.youtube.com/embed/{0}?autoplay=1";

        #endregion

        #region Progress

        public async Task<T> RunAsync<T>(Func<Task<T>> function)
        {
            try
            {
                BeginProgress();
                return await function.Invoke();
            }
            finally
            {
                EndProgress();
            }
        }

        public void BeginProgress()
        {
            if (Interlocked.Increment(ref ProgressCounter) > 0)
                TaskbarButtonService.ProgressState = TaskbarItemProgressState.Indeterminate;
        }

        public void EndProgress()
        {
            if (Interlocked.Decrement(ref ProgressCounter) == 0)
                TaskbarButtonService.ProgressState = TaskbarItemProgressState.Normal;
        }

        private int ProgressCounter = 0;

        #endregion

        #region Commands

        public void PrintPreview(IPrintableControl control)
        {
            PrintableControlLink link = new PrintableControlLink(control);
            link.Margins = new Margins(50, 50, 50, 50);

            link.ShowRibbonPrintPreview(App.Current.MainWindow);
        }

        public void ExportToExcel(IPrintableControl control)
        {
            PrintableControlLink link = new PrintableControlLink(control);
            link.Margins = new Margins(50, 50, 50, 50);

            if (ExportHelper.Export(ExportFormat.Xlsx, link.PrintingSystem) == true)
            {
                SaveFileDialogService.Filter = "Excel File (.xlsx)|*.xlsx";

                if (SaveFileDialogService.ShowDialog())
                {
                    link.ExportToXlsx(SaveFileDialogService.GetFullFileName());
                    OpenExternalApplication(SaveFileDialogService.GetFullFileName());
                }
            }
        }

        public void ExportToWord(IPrintableControl control)
        {
            PrintableControlLink link = new PrintableControlLink(control);
            link.Margins = new Margins(50, 50, 50, 50);

            if (ExportHelper.Export(ExportFormat.Docx, link.PrintingSystem) == true)
            {
                SaveFileDialogService.Filter = "Word File (.docx)|*.docx";

                if (SaveFileDialogService.ShowDialog())
                {
                    link.ExportToDocx(SaveFileDialogService.GetFullFileName());
                    OpenExternalApplication(SaveFileDialogService.GetFullFileName());
                }
            }
        }

        public void ExportToPdf(IPrintableControl control)
        {
            PrintableControlLink link = new PrintableControlLink(control);
            link.Margins = new Margins(50, 50, 50, 50);

            if (ExportHelper.Export(ExportFormat.Pdf, link.PrintingSystem) == true)
            {
                SaveFileDialogService.Filter = "PDF File (.pdf)|*.pdf";

                if (SaveFileDialogService.ShowDialog())
                {
                    link.ExportToPdf(SaveFileDialogService.GetFullFileName());
                    OpenExternalApplication(SaveFileDialogService.GetFullFileName());
                }
            }
        }

        public bool CanPlayYoutubeVideo(string key)
        {
            return !String.IsNullOrWhiteSpace(key);
        }

        public void PlayYoutubeVideo(string key)
        {
            OpenExternalApplication(String.Format(YoutubeVideoLink, key));
        }

        public bool CanOpenExternalApplication(string path)
        {
            return !String.IsNullOrWhiteSpace(path);
        }

        public void OpenExternalApplication(string path)
        {
            try
            {
                Process.Start(path);
            }
            catch (Exception e)
            {
                Journal.WriteError(e);

                MessageViewModel viewModel = MessageViewModel.FromException(e);
                Messenger.Default.Send(viewModel);
            }
        }

        public bool CanAddToRepository()
        {
            return (Parameter as ContainerBase)?.IsAdded == false;
        }

        public void AddToRepository()
        {
            switch (Parameter)
            {
                case MovieContainer movie:
                    App.Repository.Movies.Add(movie);
                    break;

                case PersonContainer person:
                    App.Repository.People.Add(person);
                    break;

                case TvShowContainer tvShow:
                    App.Repository.TvShows.Add(tvShow);
                    break;
            }
        }

        public bool CanUpdateRepository()
        {
            return (Parameter as ContainerBase)?.IsAdded == true;
        }

        public void UpdateRepository()
        {
            switch (Parameter)
            {
                case MovieContainer movie:
                    App.Repository.Movies.Update(movie);
                    break;

                case PersonContainer person:
                    App.Repository.People.Update(person);
                    break;

                case TvShowContainer tvShow:
                    App.Repository.TvShows.Update(tvShow);
                    break;
            }
        }

        public bool CanRemoveFromRepository()
        {
            return (Parameter as ContainerBase)?.IsAdded == true;
        }

        public void RemoveFromRepository()
        {
            switch (Parameter)
            {
                case MovieContainer movie:
                    App.Repository.Movies.Remove(movie);
                    break;

                case PersonContainer person:
                    App.Repository.People.Remove(person);
                    break;

                case TvShowContainer tvShow:
                    App.Repository.TvShows.Remove(tvShow);
                    break;
            }
        }

        public bool CanRemoveItemsFromRepository(IList<object> items)
        {
            return items?.Count > 0;
        }

        public void RemoveItemsFromRepository(IList<object> items)
        {
            if (MessageBoxService.ShowMessage(Properties.Resources.RemoveSelectedConfirm, Properties.Resources.Remove, 
                MessageButton.YesNo, MessageIcon.Question, MessageResult.No) != MessageResult.Yes)
                return;

            List<object> removalList = new List<object>(items);
            foreach (object item in removalList)
            {
                switch (item)
                {
                    case MovieContainer movie:
                        App.Repository.Movies.Remove(movie);
                        break;

                    case PersonContainer person:
                        App.Repository.People.Remove(person);
                        break;

                    case TvShowContainer tvShow:
                        App.Repository.TvShows.Remove(tvShow);
                        break;
                }
            }
        }

        public bool CanUpdateItems(IList<object> items)
        {
            return items?.Count > 0;
        }

        public async Task UpdateItems(IList<object> items)
        {
            if (MessageBoxService.ShowMessage(Properties.Resources.RefreshSelectedConfirm, Properties.Resources.Refresh,
                MessageButton.YesNo, MessageIcon.Question, MessageResult.No) != MessageResult.Yes)
                return;

            try
            {
                IAsyncCommand command = this.GetAsyncCommand(x => x.UpdateItems(items));
                CancellationToken cancellationToken = command.CancellationTokenSource.Token;
                BackgroundOperation.Register(Properties.Resources.Refresh, command.CancelCommand);

                foreach (object item in items)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    switch (item)
                    {
                        case MovieContainer movie:
                            await App.Repository.UpdateMovieAsync(movie, cancellationToken);
                            break;

                        case PersonContainer person:
                            await App.Repository.UpdatePersonAsync(person, cancellationToken);
                            break;

                        case TvShowContainer tvShow:
                            await App.Repository.UpdateTvShowAsync(tvShow, cancellationToken);
                            break;
                    }
                }
            }
            finally
            {
                BackgroundOperation.UnRegister(Properties.Resources.Refresh);
            }
        }

        public async Task UpdateItem(CancellationToken cancellationToken)
        {
            switch (Parameter)
            {
                case MovieContainer movie:
                    await App.Repository.UpdateMovieAsync(movie, cancellationToken);
                    break;

                case PersonContainer person:
                    await App.Repository.UpdatePersonAsync(person, cancellationToken);
                    break;

                case TvShowContainer tvShow:
                    await App.Repository.UpdateTvShowAsync(tvShow, cancellationToken);
                    break;
            }
        }

        public void OpenImageGallery(object parameter)
        {
            DialogViewModel viewModel = ViewModelSource.Create<DialogViewModel>();
            viewModel.ResizeMode = ResizeMode.NoResize;
            viewModel.WindowStyle = WindowStyle.None;
            viewModel.Parameter = parameter;

            DialogService.ShowDialog(null, Properties.Resources.ImageGallery, "ImageGalleryView", viewModel);
        }

        public bool CanShowBackgroundOperations()
        {
            return BackgroundOperation.Operations.Count > 0;
        }

        public void ShowBackgroundOperations()
        {
            DialogViewModel viewModel = ViewModelSource.Create<DialogViewModel>();
            viewModel.Parameter = BackgroundOperation.Operations;
            
            DialogService.ShowDialog(MessageButton.OK, Properties.Resources.LoadingOperations, "BackgroundOperationsView", viewModel);
        }

        #endregion
    }
}
