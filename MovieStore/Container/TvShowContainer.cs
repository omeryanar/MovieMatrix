using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LiteDB;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Search;
using TMDbLib.Objects.TvShows;

namespace MovieStore.Container
{
    public class TvShowContainer : ContainerBase
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
        public TvShow TvShow
        {
            get => Item as TvShow;
        }

        [BsonIgnore]
        public string Company
        {
            get => TvShow?.ProductionCompanies?.Select(x => x.Name).Join();
        }

        [BsonIgnore]
        public string Country
        {
            get => TvShow?.OriginCountry?.Select(x => new RegionInfo(x).EnglishName).Join();
        }

        [BsonIgnore]
        public string Network
        {
            get => TvShow?.Networks?.Select(x => x.Name).Join();
        }

        [BsonIgnore]
        public string Keywords
        {
            get => TvShow?.Keywords?.Results?.Select(x => x.Name).Join();
        }

        [BsonIgnore]
        public string Starring
        {
            get => TvShow?.Credits?.Cast?.OrderBy(x => x.Order).Take(5).Select(x => x.Name).Join();
        }

        [BsonIgnore]
        public string Creator
        {
            get => TvShow?.CreatedBy?.Select(x => x.Name).Join();
        }

        [BsonIgnore]
        public int? Runtime
        {
            get => TvShow?.EpisodeRunTime?.FirstOrDefault();
        }

        [BsonIgnore]
        public string ImdbId
        {
            get => TvShow?.ExternalIds?.ImdbId;
        }

        [BsonIgnore]
        public string ImdbLink
        {
            get => ImdbTitlePage + TvShow?.ExternalIds?.ImdbId;
        }

        [BsonIgnore]
        public string TmdbLink
        {
            get => TmdbTvShowPage + TvShow?.Id;
        }

        [BsonIgnore]
        public string TrailerLink
        {
            get => TvShow?.Videos?.Results?.FirstOrDefault()?.Key;
        }

        [BsonIgnore]
        public IEnumerable<Cast> FeaturedCast
        {
            get => TvShow?.Credits?.Cast?.OrderBy(x => x.Order).Take(5);
        }

        [BsonIgnore]
        public IEnumerable<Crew> FeaturedCrew
        {
            get => TvShow?.CreatedBy?.Select(x => new Crew { Id = x.Id, Name = x.Name, Gender = x.Gender, ProfilePath = x.ProfilePath, Job = Utilities.TvShowFeaturedJobs[0] }).
                Union(TvShow?.Credits?.Crew?.Where(x => Utilities.TvShowFeaturedJobs.Contains(x.Job)));
        }

        [BsonIgnore]
        public IEnumerable<Cast> FullCast
        {
            get => TvShow?.Credits?.Cast?.OrderBy(x => x.Order).Skip(5);
        }

        [BsonIgnore]
        public IEnumerable<Crew> FullCrew
        {
            get => TvShow?.Credits?.Crew?.Where(x => !Utilities.TvShowFeaturedJobs.Contains(x.Job))?.OrderByDescending(x => x.ProfilePath);
        }

        #endregion

        #region Translations

        public string GetName(string language)
        {
            string translation = TvShow?.Translations?.Translations?.FirstOrDefault(x => x.Iso_639_1 == language)?.Data?.Name;

            return String.IsNullOrEmpty(translation) ? (Item as SearchTv)?.Name : translation;
        }

        public string GetOverview(string language)
        {
            string translation = TvShow?.Translations?.Translations?.FirstOrDefault(x => x.Iso_639_1 == language)?.Data?.Overview;

            return String.IsNullOrEmpty(translation) ? (Item as SearchTv)?.Overview : translation;
        }

        #endregion

        public TvShowContainer() { }

        public TvShowContainer(SearchMovieTvBase item) : base(item) { }

        public const string TmdbTvShowPage = "www.themoviedb.org/tv/";
    }
}
