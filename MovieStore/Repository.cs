using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using MovieStore.Container;
using MovieStore.Helper;
using TMDbLib.Client;
using TMDbLib.Objects.Collections;
using TMDbLib.Objects.Discover;
using TMDbLib.Objects.Find;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Movies;
using TMDbLib.Objects.People;
using TMDbLib.Objects.Search;
using TMDbLib.Objects.TvShows;

namespace MovieStore
{
    public class Repository
    {
        #region Fields

        private TMDbClient TMDbClient;

        private HttpClient ImageClient;

        private HttpClient YouTubeClient;

        private LiteStorage FileStorage;

        #endregion

        #region Properties

        public ContainerCollection<MovieContainer> Movies { get; private set; }

        public ContainerCollection<PersonContainer> People { get; private set; }

        public ContainerCollection<TvShowContainer> TvShows { get; private set; }

        #endregion

        public Repository(string databaseName)
        {
            TMDbClient = new TMDbClient("1915ba01826b36e88f574596aa8a7e54", false);

            ImageClient = new HttpClient();
            ImageClient.BaseAddress = new Uri("http://image.tmdb.org/t/p/");

            YouTubeClient = new HttpClient();
            YouTubeClient.BaseAddress = new Uri("http://img.youtube.com/vi/");

            DirectoryInfo directory = Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data"));
            LiteDatabase database = new LiteDatabase(Path.Combine(directory.FullName, databaseName));
            FileStorage = database.FileStorage;

            Movies = new ContainerCollection<MovieContainer>(database, "Movies");
            People = new ContainerCollection<PersonContainer>(database, "People");
            TvShows = new ContainerCollection<TvShowContainer>(database, "TvShows");
        }

        public async Task<Stream> GetImageAsync(string path)
        {
            try
            {
                if (path.StartsWith("/"))
                    return await ImageClient.GetStreamAsync("original" + path);
                else if (!path.StartsWith("w"))
                    return await YouTubeClient.GetStreamAsync(String.Format("{0}/0.jpg", path));

                string id = path.ComputeHash();
                LiteFileInfo fileInfo = FileStorage.FindById(id);
                if (fileInfo == null)
                {
                    try
                    {
                        using (Stream stream = await ImageClient.GetStreamAsync(path))
                        {
                            if (stream != null)
                            {
                                using(MemoryStream memoryStream = new MemoryStream())
                                {
                                    stream.CopyTo(memoryStream);
                                    if (memoryStream.Length > 0)
                                    {
                                        memoryStream.Position = 0;
                                        fileInfo = FileStorage.Upload(id, path, memoryStream);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }

                if (fileInfo == null)
                    return null;

                MemoryStream cachedStream = new MemoryStream();
                fileInfo.CopyTo(cachedStream);
                cachedStream.Position = 0;

                return cachedStream;
            }
            catch (Exception)
            {
                return null;
            }
        }        

        #region Find

        public async Task<List<SearchBase>> FindAsync(string query, string language, MediaType mediaType, CancellationToken cancellationToken)
        {
            bool searchByImdbId = Regex.IsMatch(query, @"tt\d{7}") || Regex.IsMatch(query, @"nm\d{7}");
            List<SearchBase> results = new List<SearchBase>();
            List<SearchMovie> movies = null;
            List<SearchTv> tvShows = null;
            List<SearchPerson> people = null;

            if (searchByImdbId)
            {
                var tmdbResult = await TMDbClient.FindAsync(FindExternalSource.Imdb, query, language, cancellationToken);
                movies = tmdbResult.MovieResults;
                tvShows = tmdbResult.TvResults;
                people = tmdbResult.PersonResults;
            }

            switch (mediaType)
            {
                case MediaType.Movie:
                    if (!searchByImdbId)
                        movies = await SearchMovieAsync(query, language, 0, cancellationToken);

                    results.AddRange(movies.Where(x => x.Title == query).OrderByDescending(x => x.Popularity).Union(movies.Where(x => x.Title != query).OrderByDescending(x => x.Popularity)));
                    break;
                
                case MediaType.Tv:
                    if (!searchByImdbId)
                        tvShows = await SearchTvShowAsync(query, language, 0, cancellationToken);

                    results.AddRange(tvShows.Where(x => x.Name == query).OrderByDescending(x => x.Popularity).Union(tvShows.Where(x => x.Name != query).OrderByDescending(x => x.Popularity)));
                    break;
                
                case MediaType.Person:
                    if (!searchByImdbId)
                        people = await SearchPersonAsync(query, language, 0, cancellationToken);

                    results.AddRange(people.Where(x => x.Name == query).OrderByDescending(x => x.Popularity).Union(people.Where(x => x.Name != query).OrderByDescending(x => x.Popularity)));
                    break;
            }

            return results;
        }

        #endregion

        #region Search

        public async Task<List<ContainerBase>> SearchMultiAsync(string query, string language, int page, CancellationToken cancellationToken)
        {            
            var tmdbResult = await TMDbClient.SearchMultiAsync(query: query, language: language, page: page, cancellationToken: cancellationToken);
            List<ContainerBase> result = new List<ContainerBase>(tmdbResult.TotalResults);

            foreach (SearchBase item in tmdbResult.Results)
            {
                switch (item)
                {
                    case SearchMovie movie:
                        result.Add(GetContainer(movie));
                        break;

                    case SearchPerson person:
                        result.Add(GetContainer(person));
                        break;

                    case SearchTv tvShow:
                        result.Add(GetContainer(tvShow));
                        break;
                }
            }

            return result;
        }

        public async Task<List<SearchMovie>> SearchMovieAsync(string query, string language, int page, CancellationToken cancellationToken)
        {
            var tmdbResult = await TMDbClient.SearchMovieAsync(query: query, language: language, page: page, cancellationToken: cancellationToken);

            return tmdbResult.Results;
        }

        public async Task<List<SearchTv>> SearchTvShowAsync(string query, string language, int page, CancellationToken cancellationToken)
        {
            var tmdbResult = await TMDbClient.SearchTvShowAsync(query: query, language: language, page: page, cancellationToken: cancellationToken);

            return tmdbResult.Results;
        }

        public async Task<List<SearchPerson>> SearchPersonAsync(string query, string language, int page, CancellationToken cancellationToken)
        {
            var tmdbResult = await TMDbClient.SearchPersonAsync(query: query, language: language, page: page, cancellationToken: cancellationToken);

            return tmdbResult.Results;
        }

        public async Task<List<SearchKeyword>> SearchKeywordAsync(string query, int page, CancellationToken cancellationToken)
        {
            var tmdbResult = await TMDbClient.SearchKeywordAsync(query: query, page: page, cancellationToken: cancellationToken);

            return tmdbResult.Results;
        }

        public async Task<List<SearchCompany>> SearchCompanyAsync(string query, int page, CancellationToken cancellationToken)
        {
            var tmdbResult = await TMDbClient.SearchCompanyAsync(query: query, page: page, cancellationToken: cancellationToken);

            return tmdbResult.Results;
        }

        #endregion

        #region Movies

        public DiscoverMovie DiscoverMovie()
        {
            return new DiscoverMovie(TMDbClient);
        }

        public async Task<List<MovieContainer>> DiscoverMovieAsync(DiscoverMovie movie, string language, int page, CancellationToken cancellationToken)
        {
            var tmdbResult = await movie.Query(language: language, page: page, cancellationToken: cancellationToken);

            return GetContainerList(tmdbResult.Results, tmdbResult.TotalResults);
        }

        public async Task<List<MovieContainer>> GetMovieUpcomingListAsync(string language, int page, CancellationToken cancellationToken)
        {
            var tmdbResult = await TMDbClient.GetMovieUpcomingListAsync(language: language, page: page, cancellationToken: cancellationToken);

            return GetContainerList(tmdbResult.Results, tmdbResult.TotalResults);
        }

        public async Task<List<MovieContainer>> GetMovieNowPlayingListAsync(string language, int page, CancellationToken cancellationToken)
        {
            var tmdbResult = await TMDbClient.GetMovieNowPlayingListAsync(language: language, page: page, cancellationToken: cancellationToken);

            return GetContainerList(tmdbResult.Results, tmdbResult.TotalResults);
        }

        public async Task<List<MovieContainer>> GetMoviePopularListAsync(string language, int page, CancellationToken cancellationToken)
        {
            var tmdbResult = await TMDbClient.GetMoviePopularListAsync(language: language, page: page, cancellationToken: cancellationToken);

            return GetContainerList(tmdbResult.Results, tmdbResult.TotalResults);
        }

        public async Task<List<MovieContainer>> GetMovieOscarWinnersListAsync(string language, CancellationToken cancellationToken)
        {
            var tmdbResult = await TMDbClient.GetListAsync(listId: "28", language: language, cancellationToken: cancellationToken);
            List<MovieContainer> result = new List<MovieContainer>(tmdbResult.ItemCount);

            foreach (SearchMovie movie in tmdbResult.Items.OfType<SearchMovie>())
                result.Add(GetContainer(movie));

            return result;
        }

        public async Task<List<MovieContainer>> GetMovieIMDbTop250ListAsync(string language, CancellationToken cancellationToken)
        {
            var tmdbResult = await TMDbClient.GetListAsync(listId: "10265", language: language, cancellationToken: cancellationToken);
            List<MovieContainer> result = new List<MovieContainer>(tmdbResult.ItemCount);

            for (int i = 0; i < tmdbResult.Items.Count; i++)
            {
                if (tmdbResult.Items[i] is SearchMovie movie)
                {
                    MovieContainer container = GetContainer(movie);
                    result.Add(container);

                    if (container.TopRating != i + 1)
                    {
                        container.TopRating = i + 1;
                        if (container.IsAdded)
                            Movies.Update(container);
                    }
                }
            }

            return result;
        }

        public async Task<List<MovieContainer>> GetCollectionAsync(int collectionId, string language, CancellationToken cancellationToken)
        {
            var tmdbResult = await TMDbClient.GetCollectionAsync(collectionId, language, CollectionMethods.Images, cancellationToken);
            tmdbResult.Parts = tmdbResult.Parts.Where(x => x.ReleaseDate != null).OrderBy(x => x.ReleaseDate).ToList();

            return GetContainerList(tmdbResult.Parts, tmdbResult.Parts.Count);
        }

        public async Task<MovieContainer> GetMovieAsync(int movieId, CancellationToken cancellationToken)
        {
            MovieContainer container = Movies.FindById(movieId);
            if (container != null)
                return container;

            MovieMethods methods = MovieMethods.Credits | MovieMethods.Images | MovieMethods.Videos | MovieMethods.Translations | MovieMethods.Keywords;
            Movie tmdbResult = await TMDbClient.GetMovieAsync(movieId, methods, cancellationToken);
            container = new MovieContainer(tmdbResult);

            ImdbHelper.GetImdbInfo(tmdbResult.ImdbId).ContinueWith(x =>
            {
                if (x.IsCompleted && !x.IsFaulted && x.Result != null)
                {
                    container.Votes = x.Result.Resource.RatingCount;
                    container.TopRating = x.Result.Resource.TopRank;
                    container.ImdbRating = x.Result.Resource.Rating;
                }
            }, TaskScheduler.FromCurrentSynchronizationContext()).LogIfFaulted();

            return container;
        }

        public async Task UpdateMovieAsync(MovieContainer container, CancellationToken cancellationToken)
        {
            MovieMethods methods = MovieMethods.Credits | MovieMethods.Images | MovieMethods.Videos | MovieMethods.Translations | MovieMethods.Keywords;
            Movie tmdbResult = await TMDbClient.GetMovieAsync(container.Id, methods, cancellationToken);
            container.Item = tmdbResult;

            ImdbInfo imdbInfo = await ImdbHelper.GetImdbInfo(tmdbResult.ImdbId);
            container.Votes = imdbInfo.Resource.RatingCount;
            container.TopRating = imdbInfo.Resource.TopRank;
            container.ImdbRating = imdbInfo.Resource.Rating;

            Movies.Update(container);
        }

        private List<MovieContainer> GetContainerList(List<SearchMovie> list, int count)
        {
            List<MovieContainer> result = new List<MovieContainer>(count);
            foreach (SearchMovie item in list)
                result.Add(GetContainer(item));

            return result;
        }

        private MovieContainer GetContainer(SearchMovie item)
        {
            MovieContainer container = Movies.FindById(item.Id);
            if (container == null)
                container = new MovieContainer(item);

            return container;
        }

        #endregion        

        #region TvShows

        public DiscoverTv DiscoverTvShow()
        {
            return new DiscoverTv(TMDbClient);
        }

        public async Task<List<TvShowContainer>> DiscoverTvShowAsync(DiscoverTv tvShow, string language, int page, CancellationToken cancellationToken)
        {
            var tmdbResult = await tvShow.Query(language: language, page: page, cancellationToken: cancellationToken);

            return GetContainerList(tmdbResult.Results, tmdbResult.TotalResults);
        }

        public async Task<List<TvShowContainer>> GetTvShowAiringTodayAsync(string language, int page, CancellationToken cancellationToken)
        {
            var tmdbResult = await TMDbClient.GetTvShowListAsync(TvShowListType.AiringToday, language: language, page: page, cancellationToken: cancellationToken);

            return GetContainerList(tmdbResult.Results, tmdbResult.TotalResults);
        }

        public async Task<List<TvShowContainer>> GetTvShowOnTheAirAsync(string language, int page, CancellationToken cancellationToken)
        {
            var tmdbResult = await TMDbClient.GetTvShowListAsync(TvShowListType.OnTheAir, language: language, page: page, cancellationToken: cancellationToken);

            return GetContainerList(tmdbResult.Results, tmdbResult.TotalResults);
        }

        public async Task<List<TvShowContainer>> GetTvShowPopularAsync(string language, int page, CancellationToken cancellationToken)
        {
            var tmdbResult = await TMDbClient.GetTvShowListAsync(TvShowListType.Popular, language: language, page: page, cancellationToken: cancellationToken);

            return GetContainerList(tmdbResult.Results, tmdbResult.TotalResults);
        }

        public async Task<List<TvShowContainer>> GetTvShowIMDbTop250ListAsync(string language, CancellationToken cancellationToken)
        {
            var tmdbResult = await TMDbClient.GetListAsync(listId: "142134", language: language, cancellationToken: cancellationToken);
            List<TvShowContainer> result = new List<TvShowContainer>(tmdbResult.ItemCount);

            for (int i = 0; i < tmdbResult.Items.Count; i++)
            {
                if (tmdbResult.Items[i] is SearchTv tvShow)
                {
                    TvShowContainer container = GetContainer(tvShow);
                    result.Add(container);

                    if (container.TopRating != i + 1)
                    {
                        container.TopRating = i + 1;
                        if (container.IsAdded)
                            TvShows.Update(container);
                    }
                }
            }

            return result;
        }

        public async Task<TvSeason> GetTvSeasonAsync(int tvShowId, int seasonNumber, string language, CancellationToken cancellationToken)
        {
            TvSeasonMethods methods = TvSeasonMethods.Credits | TvSeasonMethods.Images | TvSeasonMethods.Videos;
            TvSeason tmdbResult = await TMDbClient.GetTvSeasonAsync(tvShowId, seasonNumber, methods, language, cancellationToken);

            return tmdbResult;
        }

        public async Task<TvShowContainer> GetTvShowAsync(int tvShowId, CancellationToken cancellationToken)
        {
            TvShowContainer container = TvShows.FindById(tvShowId);
            if (container != null)
                return container;

            TvShowMethods methods = TvShowMethods.Credits | TvShowMethods.Images | TvShowMethods.Videos | TvShowMethods.Translations | TvShowMethods.Keywords | TvShowMethods.ExternalIds;
            TvShow tmdbResult = await TMDbClient.GetTvShowAsync(tvShowId, methods, cancellationToken);
            container = new TvShowContainer(tmdbResult);

            ImdbHelper.GetImdbInfo(tmdbResult.ExternalIds.ImdbId).ContinueWith(x =>
            {
                if (x.IsCompleted && !x.IsFaulted && x.Result != null)
                {
                    container.Votes = x.Result.Resource.RatingCount;
                    container.ImdbRating = x.Result.Resource.Rating;

                    if (x.Result.Resource.OtherRanks?.Length > 0)
                        container.TopRating = x.Result.Resource.OtherRanks[0].Rank;
                    
                }
            }, TaskScheduler.FromCurrentSynchronizationContext()).LogIfFaulted();

            return container;
        }

        public async Task UpdateTvShowAsync(TvShowContainer container, CancellationToken cancellationToken)
        {
            TvShowMethods methods = TvShowMethods.Credits | TvShowMethods.Images | TvShowMethods.Videos | TvShowMethods.Translations | TvShowMethods.Keywords | TvShowMethods.ExternalIds;
            TvShow tmdbResult = await TMDbClient.GetTvShowAsync(container.Id, methods, cancellationToken);
            container.Item = tmdbResult;

            ImdbInfo imdbInfo = await ImdbHelper.GetImdbInfo(tmdbResult.ExternalIds.ImdbId);
            container.Votes = imdbInfo.Resource.RatingCount;
            container.TopRating = imdbInfo.Resource.OtherRanks[0].Rank;
            container.ImdbRating = imdbInfo.Resource.Rating;

            TvShows.Update(container);
        }

        private List<TvShowContainer> GetContainerList(List<SearchTv> list, int count)
        {
            List<TvShowContainer> result = new List<TvShowContainer>(count);
            foreach (SearchTv item in list)
                result.Add(GetContainer(item));

            return result;
        }

        private TvShowContainer GetContainer(SearchTv item)
        {
            TvShowContainer container = TvShows.FindById(item.Id);
            if (container == null)
                container = new TvShowContainer(item);

            return container;
        }

        #endregion

        #region People

        public async Task<List<PersonContainer>> GetPersonPopularAsync(string language, int page, CancellationToken cancellationToken)
        {
            var tmdbResult = await TMDbClient.GetPersonListAsync(PersonListType.Popular, language: language, page: page, cancellationToken: cancellationToken);

            return GetContainerList(tmdbResult.Results, tmdbResult.TotalResults);
        }

        public async Task<PersonContainer> GetPersonAsync(int personId, CancellationToken cancellationToken)
        {
            PersonContainer container = People.FindById(personId);
            if (container != null)
                return container;

            PersonMethods methods = PersonMethods.MovieCredits | PersonMethods.TvCredits | PersonMethods.Images | PersonMethods.ExternalIds | PersonMethods.Translations;
            Person tmdbResult = await TMDbClient.GetPersonAsync(personId, methods, cancellationToken);
            container = new PersonContainer(tmdbResult);

            return container;
        }

        public async Task UpdatePersonAsync(PersonContainer container, CancellationToken cancellationToken)
        {
            PersonMethods methods = PersonMethods.MovieCredits | PersonMethods.TvCredits | PersonMethods.Images | PersonMethods.ExternalIds | PersonMethods.Translations;
            Person tmdbResult = await TMDbClient.GetPersonAsync(container.Id, methods, cancellationToken);

            container.Item = tmdbResult;
            People.Update(container);
        }

        private List<PersonContainer> GetContainerList(List<SearchPerson> list, int count)
        {
            List<PersonContainer> result = new List<PersonContainer>(count);
            foreach (SearchPerson item in list)
                result.Add(GetContainer(item));

            return result;
        }

        private PersonContainer GetContainer(SearchPerson item)
        {
            PersonContainer container = People.FindById(item.Id);
            if (container == null)
                container = new PersonContainer(item);

            return container;
        }

        #endregion
    }
}
