using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Core;
using MovieMatrix.Helper;
using MovieMatrix.Model;
using MovieMatrix.Properties;
using MovieMatrix.Resources;
using MovieStore.Container;
using TMDbLib.Objects.Discover;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Search;
using TMDbLib.Objects.TvShows;

namespace MovieMatrix.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        public virtual ICollectionView Themes { get; set; }

        public virtual ICollectionView Languages { get; set; }

        public virtual DocumentCollection Documents { get; } = new DocumentCollection();

        #region Window

        public void Initialize()
        {
            #region Themes

            ThemeManager.RegisterMaterialThemes();
            List<Theme> themes = Theme.Themes.Where(x => !x.IsStandard).ToList();
            Themes = CollectionViewSource.GetDefaultView(themes);
            Themes.MoveCurrentTo(null);

            Themes.CurrentChanged += (s, e) =>
            {
                Theme theme = Themes.CurrentItem as Theme;
                ApplicationThemeHelper.ApplicationThemeName = theme.Name;

                Settings.Default.ThemeName = theme.Name;
            };

            Themes.MoveCurrentTo(themes.Find(x => x.Name == Settings.Default.ThemeName));

            #endregion Themes

            #region Languages

            List<CultureInfo> languages = new List<CultureInfo>
            {
                new CultureInfo("en"),
                new CultureInfo("tr")
            };
            Languages = CollectionViewSource.GetDefaultView(languages);

            Languages.CurrentChanged += (s, e) =>
            {
                CultureInfo culture = Languages.CurrentItem as CultureInfo;
                ResourceProvider.ChangeCulture(culture);

                Settings.Default.Language = culture.Name;
            };

            Properties.Resources.Culture = Thread.CurrentThread.CurrentCulture;
            Languages.MoveCurrentTo(languages.Find(x => x.Name == Settings.Default.Language));

            #endregion

            Messenger.Default.Register(this, (CloseDocumentMessage message) =>
            {
                Documents.ForceRemove(message.Document);
            });

            Messenger.Default.Register(this, (MessageViewModel message) =>
            {
                DialogService.ShowDialog(MessageButton.OK, message.Title, "MessageView", message);
            });

            Settings.Default.PropertyChanged += (s, e) =>
            {
                Settings.Default.Save();
            };
        }

        public void ChangeTheme(string name)
        {
            Themes.MoveCurrentTo(Themes.SourceCollection.OfType<Theme>().FirstOrDefault(x => x.Name == name));
        }

        public void ChangeLanguage(string name)
        {
            Languages.MoveCurrentTo(Languages.SourceCollection.OfType<CultureInfo>().FirstOrDefault(x => x.Name == name));
        }

        public bool ShowDocument(string header, string documentId = null)
        {
            DocumentViewModel viewModel = null;
            if (documentId != null)
                viewModel = Documents.OfType<DocumentViewModel>().FirstOrDefault(x => x.DocumentId == documentId);
            else
                viewModel = Documents.OfType<DocumentViewModel>().FirstOrDefault(x => x.Header.Invoke() == header);

            if (viewModel != null)
            {
                Documents.CurrentDocument = viewModel;
                return true;
            }

            return false;
        }

        public void CreateDocument(Func<string> header, string documentType, object parameter, string documentId = null)
        {
            DocumentViewModel viewModel = ViewModelSource.Create<DocumentViewModel>();
            viewModel.Header = header;
            viewModel.DocumentId = documentId;
            viewModel.DocumentType = documentType;
            viewModel.Parameter = parameter;

            AddToDocuments(viewModel);
        }

        public void AddToDocuments(DocumentViewModel viewModel)
        {
            viewModel.ParentViewModel = this;
            Documents.Add(viewModel);
        }

        public void CloseAllDocuments()
        {
            foreach (object document in Documents.ToList())
                Messenger.Default.Send(new CloseDocumentMessage(document));
        }

        #endregion

        #region Search

        public bool CanSearch(string searchText)
        {
            return !String.IsNullOrWhiteSpace(searchText);
        }

        public void Search(string searchText)
        {
            PagedDocumentViewModel<ContainerBase> viewModel = ViewModelSource.Create<PagedDocumentViewModel<ContainerBase>>();
            viewModel.FetchPage = async (x, y) => await App.Repository.SearchMultiAsync(searchText, Settings.Default.Language, x, y);
            viewModel.Header = () => String.Format("{0}: {1}", Properties.Resources.Search, searchText);
            viewModel.DocumentType = "SearchView";

            AddToDocuments(viewModel);
        }

        #endregion

        #region Movies

        public void CreateMyMovieListView()
        {
            if (ShowDocument(Properties.Resources.MyMovieList))
                return;

            CreateDocument(() => Properties.Resources.MyMovieList, "MyMovieListView", App.Repository.Movies);
        }

        public void CreatePopularMoviesView()
        {
            if (ShowDocument(Properties.Resources.PopularMovies))
                return;

            PagedDocumentViewModel<MovieContainer> viewModel = ViewModelSource.Create<PagedDocumentViewModel<MovieContainer>>();
            viewModel.FetchPage = async (x, y) => await App.Repository.GetMoviePopularListAsync(Settings.Default.Language, x, y);
            viewModel.Header = () => Properties.Resources.PopularMovies;
            viewModel.DocumentType = "SearchView";

            AddToDocuments(viewModel);
        }

        public void CreateUpcomingMoviesView()
        {
            if (ShowDocument(Properties.Resources.UpcomingMovies))
                return;

            PagedDocumentViewModel<MovieContainer> viewModel = ViewModelSource.Create<PagedDocumentViewModel<MovieContainer>>();
            viewModel.FetchPage = async (x, y) => await App.Repository.GetMovieUpcomingListAsync(Settings.Default.Language, x, y);
            viewModel.Header = () => Properties.Resources.UpcomingMovies;
            viewModel.DocumentType = "SearchView";

            AddToDocuments(viewModel);
        }

        public void CreateNowPlayingMoviesView()
        {
            if (ShowDocument(Properties.Resources.NowPlayingMovies))
                return;

            PagedDocumentViewModel<MovieContainer> viewModel = ViewModelSource.Create<PagedDocumentViewModel<MovieContainer>>();
            viewModel.FetchPage = async (x, y) => await App.Repository.GetMovieNowPlayingListAsync(Settings.Default.Language, x, y);
            viewModel.Header = () => Properties.Resources.NowPlayingMovies;
            viewModel.DocumentType = "SearchView";

            AddToDocuments(viewModel);
        }

        public async Task CreateOscarWinnersView()
        {
            if (ShowDocument(Properties.Resources.OscarWinningMovies))
                return;

            CancellationTokenSource tokenSource = this.GetAsyncCommand(x => x.CreateOscarWinnersView()).CancellationTokenSource;
            List<MovieContainer> movies = await RunAsync(() => App.Repository.GetMovieOscarWinnersListAsync(Settings.Default.Language, tokenSource.Token));
            if (movies != null)
                CreateDocument(() => Properties.Resources.OscarWinningMovies, "SearchView", movies);
        }

        public async Task CreateIMDbTop250MoviesView()
        {
            if (ShowDocument(Properties.Resources.IMDbTop250Movies))
                return;

            CancellationTokenSource tokenSource = this.GetAsyncCommand(x => x.CreateIMDbTop250MoviesView()).CancellationTokenSource;
            List<MovieContainer> movies = await RunAsync(() => App.Repository.GetMovieIMDbTop250ListAsync(Settings.Default.Language, tokenSource.Token));
            if (movies != null)
                CreateDocument(() => Properties.Resources.IMDbTop250Movies, "SearchView", movies);
        }

        public void CreateMovieDiscoverView()
        {
            if (ShowDocument(Properties.Resources.DiscoverMovies))
                return;

            DiscoverMovie movie = App.Repository.DiscoverMovie();
            PagedDocumentViewModel<MovieContainer> viewModel = ViewModelSource.Create<PagedDocumentViewModel<MovieContainer>>();
            viewModel.FetchPage = async (x, y) => await App.Repository.DiscoverMovieAsync(movie, Settings.Default.Language, x, y);
            viewModel.Header = () => Properties.Resources.DiscoverMovies;
            viewModel.DocumentType = "MovieDiscoverView";
            viewModel.Parameter = movie;

            AddToDocuments(viewModel);
        }

        public void CreateMovieGenreView(object parameter)
        {
            PagedDocumentViewModel<MovieContainer> viewModel = null;
            DiscoverMovie movie = null;

            if (ShowDocument(Properties.Resources.MovieGenres))
            {
                viewModel = Documents.CurrentDocument as PagedDocumentViewModel<MovieContainer>;
                movie = viewModel.Parameter as DiscoverMovie;
            }
            else
            {
                movie = App.Repository.DiscoverMovie();
                viewModel = ViewModelSource.Create<PagedDocumentViewModel<MovieContainer>>();
                viewModel.FetchPage = async (x, y) => await App.Repository.DiscoverMovieAsync(movie, Settings.Default.Language, x, y);
                viewModel.Header = () => Properties.Resources.MovieGenres;
                viewModel.DocumentType = "MovieGenreView";
                viewModel.Parameter = movie;

                viewModel.Parameters.VoteCount = 0d;
                viewModel.Parameters.SortBy = MovieSort.PopularityDesc;

                AddToDocuments(viewModel);
            }

            viewModel.Parameters.Genre = null;
            viewModel.Parameters.Keyword = null;
            viewModel.Parameters.Company = null;

            if (parameter != null)
            {
                viewModel.Parameters.Genre = GenreHelper.MovieGenres.Last();

                if (parameter is int genreId)
                    viewModel.Parameters.Genre = GenreHelper.MovieGenres.FirstOrDefault(x => x.Id == genreId);
                else if (parameter is Keyword keyword)
                    viewModel.Parameters.Keyword = new SearchKeyword { Id = keyword.Id, Name = keyword.Name };
                else if (parameter is ProductionCompany company)
                    viewModel.Parameters.Company = new SearchCompany { Id = company.Id, Name = company.Name };

                viewModel.Refresh();
            }
        }

        public void CreateMovieContainerView(MovieContainer container)
        {
            if (container == null)
                return;

            string documentType = "MovieSingleView";
            string documentId = String.Format("{0}:{1}", documentType, container.Id);
            if (ShowDocument(String.Empty, documentId))
                return;

            CreateDocument(() => container.GetTitle(Settings.Default.Language), documentType, container, documentId);
        }

        public async Task CreateMovieView(int movieId)
        {
            string documentType = "MovieSingleView";
            string documentId = String.Format("{0}:{1}", documentType, movieId);
            if (ShowDocument(String.Empty, documentId))
                return;

            CancellationTokenSource tokenSource = this.GetAsyncCommand(x => x.CreateMovieView(movieId)).CancellationTokenSource;
            MovieContainer container = await RunAsync(() => App.Repository.GetMovieAsync(movieId, tokenSource.Token));
            if (container != null)
                CreateDocument(() => container.GetTitle(Settings.Default.Language), documentType, container, documentId);
        }

        public async Task CreateMovieCollectionView(SearchCollection collection)
        {
            if (collection == null)
                return;

            string documentType = "MovieCollectionView";
            string documentId = String.Format("{0}:{1}", documentType, collection.Id);
            if (ShowDocument(String.Empty, documentId))
                return;

            CancellationTokenSource tokenSource = this.GetAsyncCommand(x => x.CreateMovieCollectionView(collection)).CancellationTokenSource;
            List<MovieContainer> movies = await RunAsync(() => App.Repository.GetCollectionAsync(collection.Id, Settings.Default.Language, tokenSource.Token));

            DocumentViewModel viewModel = ViewModelSource.Create<DocumentViewModel>();
            viewModel.Header = () => collection.Name;
            viewModel.DocumentId = documentId;
            viewModel.DocumentType = documentType;
            viewModel.Parameter = movies;
            viewModel.Parameters.Collection = collection;

            AddToDocuments(viewModel);
        }

        #endregion

        #region Person

        public void CreateMyPersonListView()
        {
            if (ShowDocument(Properties.Resources.MyPeopleList))
                return;

            CreateDocument(() => Properties.Resources.MyPeopleList, "MyPersonListView", App.Repository.People);
        }

        public void CreatePopularPeopleView()
        {
            if (ShowDocument(Properties.Resources.PopularPeople))
                return;

            PagedDocumentViewModel<PersonContainer> viewModel = ViewModelSource.Create<PagedDocumentViewModel<PersonContainer>>();
            viewModel.FetchPage = async (x, y) => await App.Repository.GetPersonPopularAsync(Settings.Default.Language, x, y);
            viewModel.Header = () => Properties.Resources.PopularPeople;
            viewModel.DocumentType = "SearchView";

            AddToDocuments(viewModel);
        }

        public void CreatePersonContainerView(PersonContainer container)
        {
            if (container == null)
                return;

            string documentType = "PersonSingleView";
            string documentId = String.Format("{0}:{1}", documentType, container.Id);
            if (ShowDocument(String.Empty, documentId))
                return;

            CreateDocument(() => container.Person.Name, documentType, container, documentId);
        }

        public async Task CreatePersonView(int personId)
        {
            string documentType = "PersonSingleView";
            string documentId = String.Format("{0}:{1}", documentType, personId);
            if (ShowDocument(String.Empty, documentId))
                return;

            CancellationTokenSource tokenSource = this.GetAsyncCommand(x => x.CreatePersonView(personId)).CancellationTokenSource;
            PersonContainer container = await RunAsync(() => App.Repository.GetPersonAsync(personId, tokenSource.Token));
            if (container != null)
                CreateDocument(() => container.Person.Name, documentType, container, documentId);
        }

        #endregion

        #region TvShows

        public void CreateMyTvShowListView()
        {
            if (ShowDocument(Properties.Resources.MyTVShowList))
                return;

            CreateDocument(() => Properties.Resources.MyTVShowList, "MyTvShowListView", App.Repository.TvShows);
        }

        public void CreatePopularTvShowsView()
        {
            if (ShowDocument(Properties.Resources.PopularTVShows))
                return;

            PagedDocumentViewModel<TvShowContainer> viewModel = ViewModelSource.Create<PagedDocumentViewModel<TvShowContainer>>();
            viewModel.FetchPage = async (x, y) => await App.Repository.GetTvShowPopularAsync(Settings.Default.Language, x, y);
            viewModel.Header = () => Properties.Resources.PopularTVShows;
            viewModel.DocumentType = "SearchView";

            AddToDocuments(viewModel);
        }

        public void CreateAiringTodayTvShowsView()
        {
            if (ShowDocument(Properties.Resources.TVShowsAiringToday))
                return;

            PagedDocumentViewModel<TvShowContainer> viewModel = ViewModelSource.Create<PagedDocumentViewModel<TvShowContainer>>();
            viewModel.FetchPage = async (x, y) => await App.Repository.GetTvShowAiringTodayAsync(Settings.Default.Language, x, y);
            viewModel.Header = () => Properties.Resources.TVShowsAiringToday;
            viewModel.DocumentType = "SearchView";

            AddToDocuments(viewModel);
        }

        public void CreateOnTheAirTvShowsView()
        {
            if (ShowDocument(Properties.Resources.TVShowsAiringThisWeek))
                return;

            PagedDocumentViewModel<TvShowContainer> viewModel = ViewModelSource.Create<PagedDocumentViewModel<TvShowContainer>>();
            viewModel.FetchPage = async (x, y) => await App.Repository.GetTvShowOnTheAirAsync(Settings.Default.Language, x, y);
            viewModel.Header = () => Properties.Resources.TVShowsAiringThisWeek;
            viewModel.DocumentType = "SearchView";

            AddToDocuments(viewModel);
        }

        public void CreateTvShowDiscoverView()
        {
            if (ShowDocument(Properties.Resources.DiscoverTvShows))
                return;

            DiscoverTv tvShow = App.Repository.DiscoverTvShow();
            PagedDocumentViewModel<TvShowContainer> viewModel = ViewModelSource.Create<PagedDocumentViewModel<TvShowContainer>>();
            viewModel.FetchPage = async (x, y) => await App.Repository.DiscoverTvShowAsync(tvShow, Settings.Default.Language, x, y);
            viewModel.Header = () => Properties.Resources.DiscoverTvShows;
            viewModel.DocumentType = "TvShowDiscoverView";
            viewModel.Parameter = tvShow;

            AddToDocuments(viewModel);
        }

        public async Task CreateIMDbTop250TvShowsView()
        {
            if (ShowDocument(Properties.Resources.IMDbTop250TVShows))
                return;

            CancellationTokenSource tokenSource = this.GetAsyncCommand(x => x.CreateIMDbTop250TvShowsView()).CancellationTokenSource;
            List<TvShowContainer> tvShows = await RunAsync(() => App.Repository.GetTvShowIMDbTop250ListAsync(Settings.Default.Language, tokenSource.Token));
            if (tvShows != null)
                CreateDocument(() => Properties.Resources.IMDbTop250TVShows, "SearchView", tvShows);
        }

        public void CreateTvShowGenreView(object parameter)
        {
            PagedDocumentViewModel<TvShowContainer> viewModel = null;
            DiscoverTv tvShow = null;

            if (ShowDocument(Properties.Resources.TvShowGenres))
            {
                viewModel = Documents.CurrentDocument as PagedDocumentViewModel<TvShowContainer>;
                tvShow = viewModel.Parameter as DiscoverTv;
            }
            else
            {
                tvShow = App.Repository.DiscoverTvShow();
                viewModel = ViewModelSource.Create<PagedDocumentViewModel<TvShowContainer>>();
                viewModel.FetchPage = async (x, y) => await App.Repository.DiscoverTvShowAsync(tvShow, Settings.Default.Language, x, y);
                viewModel.Header = () => Properties.Resources.TvShowGenres;
                viewModel.DocumentType = "TvShowGenreView";
                viewModel.Parameter = tvShow;

                viewModel.Parameters.VoteCount = 0d;
                viewModel.Parameters.SortBy = TvShowSort.PopularityDesc;

                AddToDocuments(viewModel);
            }

            viewModel.Parameters.Genre = null;
            viewModel.Parameters.Keyword = null;
            viewModel.Parameters.Network = null;
            viewModel.Parameters.Company = null;

            if (parameter != null)
            {
                viewModel.Parameters.Genre = GenreHelper.TvShowGenres.Last();

                if (parameter is int genreId)
                    viewModel.Parameters.Genre = GenreHelper.TvShowGenres.FirstOrDefault(x => x.Id == genreId);
                else if (parameter is NetworkBase network)
                    viewModel.Parameters.Network = new Network { Id = network.Id, Name = network.Name };
                else if (parameter is Keyword keyword)
                    viewModel.Parameters.Keyword = new SearchKeyword { Id = keyword.Id, Name = keyword.Name };
                else if (parameter is ProductionCompany company)
                    viewModel.Parameters.Company = new SearchCompany { Id = company.Id, Name = company.Name };

                viewModel.Refresh();
            }
        }

        public void CreateTvShowContainerView(TvShowContainer container)
        {
            if (container == null)
                return;

            string documentType = "TvShowSingleView";
            string documentId = String.Format("{0}:{1}", documentType, container.Id);
            if (ShowDocument(String.Empty, documentId))
                return;

            CreateDocument(() => container.GetName(Settings.Default.Language), documentType, container, documentId);
        }

        public async Task CreateTvShowView(int tvShowId)
        {
            string documentType = "TvShowSingleView";
            string documentId = String.Format("{0}:{1}", documentType, tvShowId);
            if (ShowDocument(String.Empty, documentId))
                return;

            CancellationTokenSource tokenSource = this.GetAsyncCommand(x => x.CreateTvShowView(tvShowId)).CancellationTokenSource;
            TvShowContainer container = await RunAsync(() => App.Repository.GetTvShowAsync(tvShowId, tokenSource.Token));
            if (container != null)
                CreateDocument(() => container.GetName(Settings.Default.Language), documentType, container, documentId);
        }

        public async Task CreateTvShowSeasonView(SearchTvSeason tvSeason)
        {
            if (tvSeason == null)
                return;

            string documentType = "TvShowSeasonView";
            string documentId = String.Format("{0}:{1}:{2}", documentType, tvSeason.ShowId, tvSeason.SeasonNumber);
            if (ShowDocument(String.Empty, documentId))
                return;

            CancellationTokenSource tokenSource = this.GetAsyncCommand(x => x.CreateTvShowSeasonView(tvSeason)).CancellationTokenSource;
            TvSeason season = await RunAsync(() => App.Repository.GetTvSeasonAsync(tvSeason.ShowId, tvSeason.SeasonNumber, Settings.Default.Language, tokenSource.Token));
            if (season != null)
                CreateDocument(() => String.Format("{0}: {1}", tvSeason.ShowName, season.Name), documentType, season, documentId);
        }

        #endregion

        #region Tools

        public void OpenSubtitleSearch(object parameter)
        {
            SubtitleSearchViewModel viewModel = ViewModelSource.Create<SubtitleSearchViewModel>();
            viewModel.ParentViewModel = this;
            viewModel.Parameter = parameter;

            DialogService.ShowDialog(MessageButton.OK, Properties.Resources.SearchSubtitles, "SubtitleSearchView", viewModel);
        }

        public async Task CreateImportView(MediaType mediaType)
        {
            ImportViewModel viewModel = ViewModelSource.Create<ImportViewModel>();
            viewModel.ParentViewModel = this;
            viewModel.MediaType = mediaType;

            DialogService.ShowDialog(null, Properties.Resources.Import, "ImportView", viewModel);
            if (viewModel.SelectedMediaFileInfoList != null)
            {
                try
                {
                    BeginProgress();
                    IAsyncCommand command = this.GetAsyncCommand(x => x.CreateImportView(mediaType));
                    CancellationToken cancellationToken = command.CancellationTokenSource.Token;
                    BackgroundOperation.Register(Properties.Resources.ImportWizard, command.CancelCommand);

                    List<ContainerBase> newItems = new List<ContainerBase>();
                    List<ContainerBase> existingItems = new List<ContainerBase>();

                    if (viewModel.MediaType == MediaType.Movie)
                    {
                        foreach (MediaFileInfo info in viewModel.SelectedMediaFileInfoList)
                        {
                            if (cancellationToken.IsCancellationRequested)
                                break;

                            SearchBase selectedItem = info.SelectedMediaItem as SearchBase;
                            if (selectedItem != null)
                            {
                                MovieContainer container = await App.Repository.GetMovieAsync(selectedItem.Id, cancellationToken);
                                if (container != null)
                                {
                                    container.Seen = info.PersonalInfo.Seen;
                                    container.Favorite = info.PersonalInfo.Favorite;
                                    container.Watchlist = info.PersonalInfo.Watchlist;
                                    container.LocalPath = info.PersonalInfo.LocalPath;
                                    container.PersonalRating = info.PersonalInfo.PersonalRating;

                                    if (container.IsAdded == false)
                                        newItems.Add(container);
                                    else
                                        existingItems.Add(container);
                                }
                            }
                        }

                        foreach (MovieContainer movieContainer in newItems)
                            App.Repository.Movies.Add(movieContainer);

                        foreach (MovieContainer movieContainer in existingItems)
                            App.Repository.Movies.Update(movieContainer);
                    }
                    else if (viewModel.MediaType == MediaType.Tv)
                    {
                        foreach (MediaFileInfo info in viewModel.SelectedMediaFileInfoList)
                        {
                            if (cancellationToken.IsCancellationRequested)
                                break;

                            SearchBase selectedItem = info.SelectedMediaItem as SearchBase;
                            if (selectedItem != null)
                            {
                                TvShowContainer container = await App.Repository.GetTvShowAsync(selectedItem.Id, cancellationToken);
                                if (container != null)
                                {
                                    container.Seen = info.PersonalInfo.Seen;
                                    container.Favorite = info.PersonalInfo.Favorite;
                                    container.Watchlist = info.PersonalInfo.Watchlist;
                                    container.LocalPath = info.PersonalInfo.LocalPath;
                                    container.PersonalRating = info.PersonalInfo.PersonalRating;

                                    if (container.IsAdded == false)
                                        newItems.Add(container);
                                    else
                                        existingItems.Add(container);
                                }
                            }
                        }

                        foreach (TvShowContainer tvShowContainer in newItems)
                            App.Repository.TvShows.Add(tvShowContainer);

                        foreach (TvShowContainer tvShowContainer in existingItems)
                            App.Repository.TvShows.Update(tvShowContainer);
                    }
                    else if (viewModel.MediaType == MediaType.Person)
                    {
                        foreach (MediaFileInfo info in viewModel.SelectedMediaFileInfoList)
                        {
                            if (cancellationToken.IsCancellationRequested)
                                break;

                            SearchBase selectedItem = info.SelectedMediaItem as SearchBase;
                            if (selectedItem != null)
                            {
                                PersonContainer container = await App.Repository.GetPersonAsync(selectedItem.Id, cancellationToken);
                                if (container != null)
                                {
                                    container.Favorite = info.PersonalInfo.Favorite;
                                    container.LocalPath = info.PersonalInfo.LocalPath;

                                    if (container.IsAdded == false)
                                        newItems.Add(container);
                                    else
                                        existingItems.Add(container);
                                }
                            }
                        }

                        foreach (PersonContainer personContainer in newItems)
                            App.Repository.People.Add(personContainer);

                        foreach (PersonContainer personContainer in existingItems)
                            App.Repository.People.Update(personContainer);
                    }
                }
                finally
                {
                    EndProgress();
                    BackgroundOperation.UnRegister(Properties.Resources.ImportWizard);
                }
            }
        }

        public async Task RefreshAll(MediaType mediaType)
        {
            if (MessageBoxService.ShowMessage(Properties.Resources.RefreshAllConfirm, Properties.Resources.RefreshAll,
                MessageButton.YesNo, MessageIcon.Question, MessageResult.No) != MessageResult.Yes)
                return;

            try
            {
                BeginProgress();
                IAsyncCommand command = this.GetAsyncCommand(x => x.RefreshAll(mediaType));
                CancellationToken cancellationToken = command.CancellationTokenSource.Token;
                BackgroundOperation.Register(Properties.Resources.RefreshAll, command.CancelCommand);

                switch (mediaType)
                {
                    case MediaType.Movie:
                        foreach (MovieContainer container in App.Repository.Movies)
                        {
                            if (cancellationToken.IsCancellationRequested)
                                break;

                            await App.Repository.UpdateMovieAsync(container, cancellationToken);
                        }
                        break;

                    case MediaType.Tv:
                        foreach (TvShowContainer container in App.Repository.TvShows)
                        {
                            if (cancellationToken.IsCancellationRequested)
                                break;

                            await App.Repository.UpdateTvShowAsync(container, cancellationToken);
                        }
                        break;

                    case MediaType.Person:
                        foreach (PersonContainer container in App.Repository.People)
                        {
                            if (cancellationToken.IsCancellationRequested)
                                break;

                            await App.Repository.UpdatePersonAsync(container, cancellationToken);
                        }
                        break;
                }
            }
            finally
            {
                EndProgress();
                BackgroundOperation.UnRegister(Properties.Resources.RefreshAll);
            }
        }

        #endregion
    }
}
