using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;
using TMDbLib.Objects.People;
using TMDbLib.Objects.Search;

namespace MovieStore.Container
{
    public class PersonContainer : ContainerBase
    {
        #region Personal

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

        #endregion

        #region Readonly

        [BsonIgnore]
        public Person Person
        {
            get => Item as Person;
        }

        [BsonIgnore]
        public int? Age
        {
            get => Person?.Deathday == null ?  (int?)((DateTime.Today - Person?.Birthday)?.TotalDays / 365.25) : 
                (int?)((Person?.Deathday - Person?.Birthday)?.TotalDays / 365.25);
        }

        [BsonIgnore]
        public string ImdbLink
        {
            get => ImdbNamePage + Person?.ImdbId;
        }

        [BsonIgnore]
        public string TmdbLink
        {
            get => TmdbPersonPage + Person?.Id;
        }

        [BsonIgnore]
        public string FacebookLink
        {
            get => FaceBookPage + Person?.ExternalIds?.FacebookId;
        }

        [BsonIgnore]
        public string TwitterLink
        {
            get => TwitterPage + Person?.ExternalIds?.TwitterId;
        }

        [BsonIgnore]
        public string InstagramLink
        {
            get => InstagramPage + Person?.ExternalIds?.InstagramId;
        }

        [BsonIgnore]
        public IEnumerable<SearchMovieTvBase> KnownFor
        {
            get => Person?.KnownForDepartment == "Acting" ? Person?.MovieCredits?.Cast?.Cast<SearchMovieTvBase>().Union(Person?.TvCredits?.Cast?.Cast<SearchMovieTvBase>())?.OrderByDescending(x => x.VoteAverage * x.VoteCount).Take(8)
                : MovieKnownForCredits?.Cast<SearchMovieTvBase>().Union(TvKnownForCredits?.Cast<SearchMovieTvBase>())?.OrderByDescending(x => x.VoteAverage * x.VoteCount).Take(8);
        }

        [BsonIgnore]
        public IEnumerable<MovieJob> MovieCredits
        {
            get => Person?.KnownForDepartment == "Acting" ? MovieActingCredits?.Union(Person?.MovieCredits?.Crew) :
                Person?.MovieCredits?.Crew?.Union(MovieActingCredits);
        }

        [BsonIgnore]
        public IEnumerable<TvJob> TvCredits
        {
            get => Person?.KnownForDepartment == "Acting" ? TvActingCredits?.Union(Person?.TvCredits?.Crew) :
                Person?.TvCredits?.Crew?.Union(TvActingCredits);
        }

        private IEnumerable<MovieJob> MovieActingCredits
        {
            get => Person?.MovieCredits?.Cast?.Select(x => new MovieJob { Id = x.Id, Title = x.Title, Department = "Acting", Job = x.Character, ReleaseDate = x.ReleaseDate });
        }

        private IEnumerable<MovieJob> MovieKnownForCredits
        {
            get => Person?.MovieCredits?.Crew?.Where(x => x.Department == Person?.KnownForDepartment).GroupBy(x => new { x.Id, x.Title, x.PosterPath, x.VoteCount, x.VoteAverage }).
               Select(x => new MovieJob { Id = x.Key.Id, Title = x.Key.Title, PosterPath = x.Key.PosterPath, VoteCount = x.Key.VoteCount, VoteAverage = x.Key.VoteAverage, Job = String.Join(Utilities.ListSeparator, x.OrderBy(y => y.Job).Select(z => z.Job)) });
        }

        private IEnumerable<TvJob> TvActingCredits
        {
            get => Person?.TvCredits?.Cast?.Select(x => new TvJob { Id = x.Id, Name = x.Name, Department = "Acting", Job = x.Character, FirstAirDate = x.FirstAirDate, EpisodeCount = x.EpisodeCount });
        }

        private IEnumerable<TvJob> TvKnownForCredits
        {
            get => Person?.TvCredits?.Crew?.Where(x => x.Department == Person?.KnownForDepartment).GroupBy(x => new { x.Id, x.Name, x.PosterPath, x.VoteCount, x.VoteAverage }).
                Select(x => new TvJob { Id = x.Key.Id, Name = x.Key.Name, PosterPath = x.Key.PosterPath, VoteCount = x.Key.VoteCount, VoteAverage = x.Key.VoteAverage, Job = String.Join(Utilities.ListSeparator, x.OrderBy(y => y.Job).Select(z => z.Job)) });
        }

        #endregion

        #region Translations

        public string GetBiography(string language)
        {
            string translation = Person?.Translations?.Translations?.FirstOrDefault(x => x.Iso_639_1 == language)?.Data?.Biography;

            return String.IsNullOrEmpty(translation) ? Person?.Biography : translation;
        }

        #endregion

        public PersonContainer() { }

        public PersonContainer(SearchBase item) : base(item) { }

        public const string ImdbNamePage = "www.imdb.com/name/";

        public const string TmdbPersonPage = "www.themoviedb.org/person/";
    }
}
