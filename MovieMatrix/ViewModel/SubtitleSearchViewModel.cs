using System;
using System.IO;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using MovieStore;
using MovieStore.Container;
using SubtitleDownloader;

namespace MovieMatrix.ViewModel
{
    public class SubtitleSearchViewModel : DialogViewModel
    {
        public virtual string Language { get; set; }

        public virtual int? SeasonNumber { get; set; }

        public virtual int? EpisodeNumber { get; set; }

        public virtual object SearchResult { get; set; }

        public virtual bool IsLoading { get; set; }

        public virtual string Name { get; set; }

        public virtual string ImdbId { get; set; }

        public virtual string LocalPath { get; set; }

        public virtual string FilePath { get; set; }

        public virtual bool ShowSeasonAndEpisodeNumber { get; set; }

        public bool CanSearchByName()
        {
            return !String.IsNullOrWhiteSpace(Name);
        }

        public async Task SearchByName()
        {
            try
            {
                IsLoading = true;
                SearchResult = await SubtitleClient.SearchSubtitlesFromQuery(Language, Name, SeasonNumber, EpisodeNumber);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public bool CanSearchByImdbId()
        {
            return !String.IsNullOrWhiteSpace(ImdbId);
        }

        public async Task SearchByImdbId()
        {
            try
            {
                IsLoading = true;
                SearchResult = await SubtitleClient.SearchSubtitlesFromImdb(Language, ImdbId, SeasonNumber, EpisodeNumber);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public bool CanSearchByFile()
        {
            return !String.IsNullOrWhiteSpace(FilePath) && File.Exists(FilePath);
        }

        public async Task SearchByFile()
        {
            try
            {
                IsLoading = true;
                SearchResult = await SubtitleClient.SearchSubtitlesFromFile(Language, FilePath);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task Download(Subtitle subtitle)
        {
            string path = null;
            if (!String.IsNullOrWhiteSpace(FilePath))
            {
                string folderPath = Path.GetDirectoryName(FilePath);
                if (Directory.Exists(folderPath))
                    path = folderPath;
            }
            else if (!String.IsNullOrWhiteSpace(LocalPath))
            {
                if (Directory.Exists(LocalPath))
                    path = LocalPath;
            }

            SaveFileDialogService.Filter = SubtitleFilter;
            if (SaveFileDialogService.ShowDialog(null, path, subtitle.SubtitleFileName))
            {
                try
                {
                    await SubtitleClient.DownloadSubtitleToPath(SaveFileDialogService.File.DirectoryName, subtitle, SaveFileDialogService.SafeFileName());
                }
                catch (Exception e)
                {
                    Journal.WriteError(e);

                    MessageViewModel viewModel = MessageViewModel.FromException(e);
                    Messenger.Default.Send(viewModel);
                }
            }
        }

        public SubtitleSearchViewModel()
        {
            SubtitleClient = new SubtitleClient();
            Language = Properties.Resources.Culture.ThreeLetterISOLanguageName;
        }

        protected virtual void OnParameterChanged()
        {
            if (Parameter is MovieContainer movieContainer)
            {
                Name = movieContainer.Movie?.Title;
                ImdbId = movieContainer.Movie?.ImdbId?.Substring(2);
                LocalPath = movieContainer.LocalPath;
            }
            else if (Parameter is TvShowContainer tvShowContainer)
            {
                Name = tvShowContainer.TvShow?.Name;
                ImdbId = tvShowContainer.TvShow?.ExternalIds?.ImdbId?.Substring(2);
                LocalPath = tvShowContainer.LocalPath;
                ShowSeasonAndEpisodeNumber = true;
            }
        }

        private SubtitleClient SubtitleClient;
    }
}
