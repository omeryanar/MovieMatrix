using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using MovieMatrix.Helper;
using MovieMatrix.Model;
using MovieStore;
using TMDbLib.Objects.General;

namespace MovieMatrix.ViewModel
{
    public class ImportViewModel : DialogViewModel
    {
        public virtual string FilePath { get; set; }

        public virtual string FolderPath { get; set; }

        public virtual double Progress { get; set; }

        public virtual bool IsLoading { get; set; }

        public virtual bool IsCompleted { get; set; }

        public virtual bool UseExcelFileSource { get; set; } = true;

        public virtual bool UseParentFolderName { get; set; }

        public virtual DataTable DataTable { get; set; }

        public virtual MediaType MediaType { get; set; } = MediaType.Movie;        

        public virtual List<MediaFileInfo> MediaFileInfoList { get; set; }

        public virtual List<MediaFileInfo> SelectedMediaFileInfoList { get; set; } = new List<MediaFileInfo>();

        public virtual CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();

        #region Columns

        public virtual string NameColumn { get; set; }

        public virtual string PersonalRatingColumn { get; set; }

        public virtual string SeenColumn { get; set; }

        public virtual string FavoriteColumn { get; set; }

        public virtual string WatchlistColumn { get; set; }

        public virtual string LocalPathColumn { get; set; }

        #endregion

        public void SetSourcePath()
        {
            if (UseExcelFileSource)
            {
                OpenFileDialogService.Filter = ExcelFilter;
                if (OpenFileDialogService.ShowDialog() == true)
                    FilePath = OpenFileDialogService.GetFullFileName();
            }
            else
            {
                OpenFolderDialogService.Title = Properties.Resources.Import;
                if (OpenFolderDialogService.ShowDialog() == true)
                    FolderPath = OpenFolderDialogService.Folder.Path;
            }
        }

        public void Remove(IList<object> rows)
        {
            if (MessageBoxService.ShowMessage(Properties.Resources.RemoveSelectedConfirm, Properties.Resources.Remove,
                MessageButton.YesNo, MessageIcon.Question, MessageResult.No) != MessageResult.Yes)
                return;

            MediaFileInfoList = MediaFileInfoList.FindAll(x => !rows.Contains(x));
        }

        public async void Next(int pageIndex)
        {
            if (pageIndex == 0)
            {
                try
                {
                    IsLoading = true;

                    if (UseExcelFileSource)
                        DataTable = await ExcelHelper.CreateDataTableFromExcel(FilePath);
                    else
                        MediaFileInfoList = await MediaFileInfo.Create(FolderPath, UseParentFolderName, MediaType);
                }
                finally
                {
                    IsLoading = false;
                }
            }
            else if (pageIndex == 1 || pageIndex == 2)
            {
                if (UseExcelFileSource)
                {
                    List<MediaFileInfo> mediaFileInfoList = new List<MediaFileInfo>();
                    for (int i = 0; i < DataTable.Rows.Count; i++)
                    {
                        MediaFileInfo mediaFileInfo = MediaFileInfo.Create();
                        mediaFileInfo.MediaItemName = DataTable.Rows[i].GetDataCellValue<String>(NameColumn);

                        if (String.IsNullOrWhiteSpace(mediaFileInfo.MediaItemName))
                            continue;

                        PersonalInfo personalInfo = new PersonalInfo();                        
                        personalInfo.Seen = DataTable.Rows[i].GetDataCellValue<Boolean>(SeenColumn);
                        personalInfo.Favorite = DataTable.Rows[i].GetDataCellValue<Boolean>(FavoriteColumn);
                        personalInfo.Watchlist = DataTable.Rows[i].GetDataCellValue<Boolean>(WatchlistColumn);
                        personalInfo.LocalPath = DataTable.Rows[i].GetDataCellValue<String>(LocalPathColumn);
                        personalInfo.PersonalRating = DataTable.Rows[i].GetDataCellValue<Double>(PersonalRatingColumn);
                        mediaFileInfo.PersonalInfo = personalInfo;

                        mediaFileInfoList.Add(mediaFileInfo);
                    }

                    MediaFileInfoList = mediaFileInfoList;
                }

                try
                {
                    IsLoading = true;

                    for (int i = 0; i < MediaFileInfoList.Count; i++)
                    {
                        double percent = (double)(i + 1) / MediaFileInfoList.Count;
                        Progress = Math.Floor(percent * 100);

                        MediaFileInfo mediaFileInfo = MediaFileInfoList[i];
                        if (String.IsNullOrWhiteSpace(mediaFileInfo.MediaItemName))
                            continue;

                        if (CancellationTokenSource.IsCancellationRequested)
                            break;

                        mediaFileInfo.FoundMediaItems = await App.Repository.FindAsync(mediaFileInfo.MediaItemName, Properties.Settings.Default.Language,
                            MediaType, CancellationTokenSource.Token);

                        if (mediaFileInfo.FoundMediaItems.Count > 0)
                            mediaFileInfo.SelectedMediaItem = mediaFileInfo.FoundMediaItems[0];

                    }
                }
                catch (TaskCanceledException)
                {
                    return;
                }
                finally
                {
                    IsLoading = false;
                }
            }
            else if (pageIndex == 3)
            {
                IsCompleted = SelectedMediaFileInfoList.Count > 0;
            }
        }

        public void Finish()
        {
            SelectedMediaFileInfoList.RemoveRange(x => x.SelectedMediaItem == null);
        }

        public void Cancel()
        {
            CancellationTokenSource.Cancel();
            SelectedMediaFileInfoList = null;
        }
    }
}
