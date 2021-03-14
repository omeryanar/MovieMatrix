using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Movies;
using TMDbLib.Objects.Search;

namespace MovieStore.Container
{
    public class MovieContainer : ContainerBase
    {
        #region IMDb

        public int Votes
        {
            get { return votes; }
            set
            {
                if (votes != value)
                {
                    votes = value;
                    OnPropertyChanged(nameof(Votes));
                }
            }
        }
        private int votes;

        public int TopRating
        {
            get { return topRating; }
            set
            {
                if (topRating != value)
                {
                    topRating = value;
                    OnPropertyChanged(nameof(TopRating));
                }
            }
        }
        private int topRating;

        public double ImdbRating
        {
            get { return imdbRating; }
            set
            {
                if (imdbRating != value)
                {
                    imdbRating = value;
                    OnPropertyChanged(nameof(ImdbRating));
                }
            }
        }
        private double imdbRating;

        #endregion

        #region Personal

        public bool Seen
        {
            get { return seen; }
            set
            {
                if (seen != value)
                {
                    seen = value;
                    OnPropertyChanged(nameof(Seen));
                }
            }
        }
        private bool seen;

        public bool Favorite
        {
            get { return favorite; }
            set
            {
                if (favorite != value)
                {
                    favorite = value;
                    OnPropertyChanged(nameof(Favorite));
                }
            }
        }
        private bool favorite;

        public bool Watchlist
        {
            get { return watchlist; }
            set
            {
                if (watchlist != value)
                {
                    watchlist = value;
                    OnPropertyChanged(nameof(Watchlist));
                }
            }
        }
        private bool watchlist;

        public double PersonalRating
        {
            get { return personalRating; }
            set
            {
                if (personalRating != value)
                {
                    personalRating = value;
                    OnPropertyChanged(nameof(PersonalRating));
                }
            }
        }
        private double personalRating;

        #endregion

        #region Readonly

        [BsonIgnore]
        public Movie Movie
        {
            get => Item as Movie;
        }

        [BsonIgnore]
        public string Company
        {
            get => Movie?.ProductionCompanies?.Select(x => x.Name).Join();
        }

        [BsonIgnore]
        public string Country
        {
            get => Movie?.ProductionCountries?.Select(x => x.Name).Join();
        }

        [BsonIgnore]
        public string Keywords
        {
            get => Movie?.Keywords?.Keywords?.Select(x => x.Name).Join();
        }

        [BsonIgnore]
        public string Starring
        {
            get => Movie?.Credits?.Cast?.OrderBy(x => x.Order).Take(5).Select(x => x.Name).Join();
        }

        [BsonIgnore]
        public string Director
        {
            get => Movie?.Credits?.Crew?.Where(x => Utilities.MovieFeaturedJobs[0] == x.Job).Select(x => x.Name).Join();
        }

        [BsonIgnore]
        public string Writer
        {
            get => Movie?.Credits?.Crew?.Where(x => Utilities.MovieFeaturedJobs.Skip(1).Contains(x.Job)).Select(x => x.Name).Join();
        }

        [BsonIgnore]
        public string ImdbLink
        {
            get => ImdbTitlePage + Movie?.ImdbId;
        }

        [BsonIgnore]
        public string TmdbLink
        {
            get => TmdbMoviePage + Movie?.Id;
        }

        [BsonIgnore]
        public string TrailerLink
        {
            get => Movie?.Videos?.Results?.FirstOrDefault()?.Key;
        }        

        [BsonIgnore]
        public IEnumerable<Cast> FeaturedCast
        {
            get => Movie?.Credits?.Cast?.OrderBy(x => x.Order).Take(5);
        }

        [BsonIgnore]
        public IEnumerable<Crew> FeaturedCrew
        {
            get => Movie?.Credits?.Crew?.Where(x => Utilities.MovieFeaturedJobs.Contains(x.Job)).GroupBy(x => new { x.Id, x.Name, x.Gender, x.ProfilePath }).
                Select(x => new Crew { Id = x.Key.Id, Name = x.Key.Name, Gender = x.Key.Gender, ProfilePath = x.Key.ProfilePath, Job = String.Join(Utilities.ListSeparator, x.OrderBy(y => y.Job).Select(z => z.Job)) }).OrderBy(x => x.Job);
        }

        [BsonIgnore]
        public IEnumerable<Cast> FullCast
        {
            get => Movie?.Credits?.Cast?.OrderBy(x => x.Order).Skip(5);
        }

        [BsonIgnore]
        public IEnumerable<Crew> FullCrew
        {
            get => Movie?.Credits?.Crew?.Where(x => !Utilities.MovieFeaturedJobs.Contains(x.Job))?.OrderByDescending(x => x.ProfilePath);
        }

        #endregion

        #region Translations

        public string GetTitle(string language)
        {
            string translation = Movie?.Translations?.Translations?.FirstOrDefault(x => x.Iso_639_1 == language)?.Data?.Title;

            return String.IsNullOrEmpty(translation) ? (Item as SearchMovie)?.Title : translation;
        }

        public string GetOverview(string language)
        {
            string translation = Movie?.Translations?.Translations?.FirstOrDefault(x => x.Iso_639_1 == language)?.Data?.Overview;

            return String.IsNullOrEmpty(translation) ? (Item as SearchMovie)?.Overview : translation;
        }

        #endregion

        public MovieContainer() { }

        public MovieContainer(SearchMovieTvBase item) : base(item) { }

        public const string TmdbMoviePage = "www.themoviedb.org/movie/";
    }
}
