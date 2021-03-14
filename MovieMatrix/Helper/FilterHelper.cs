using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Dynamic;
using System.Linq;
using DevExpress.Data.Filtering;
using DevExpress.Mvvm;
using DevExpress.Xpf.Core.FilteringUI;
using MovieStore;
using TMDbLib.Objects.General;

namespace MovieMatrix.Helper
{
    public static class FilterHelper
    {
        public static ExpandoObject UniqueValues { get; } = new ExpandoObject();

        public static PredefinedFilterCollection GetPredefinedFilters(MediaType mediaType, string fieldName)
        {
            string key = GetFilterKey(mediaType, fieldName);
            if (PredefinedFilterCache.ContainsKey(key))
                return PredefinedFilterCache[key];

            PredefinedFilterCollection predefinedFilters = null;
            IEnumerable<string> uniqueValues = GetUniqueValues(mediaType, fieldName);
            if (uniqueValues != null)
            {
                predefinedFilters = new PredefinedFilterCollection();
                PredefinedFilterCache.Add(key, predefinedFilters);

                foreach (string value in uniqueValues)
                {
                    PredefinedFilter predefinedFilter = new PredefinedFilter { Name = value };
                    predefinedFilter.Filter = CriteriaOperator.Parse(String.Format("Contains([{0}], '{1}')", fieldName, value.Replace("'", "''")));

                    predefinedFilters.Add(predefinedFilter);
                }
            }

            return predefinedFilters;
        }

        public static IEnumerable<string> GetUniqueValues(MediaType mediaType, string fieldName)
        {
            string key = GetFilterKey(mediaType, fieldName);
            if (UniqueValuesCache.ContainsKey(key))
                return UniqueValuesCache[key];

            IEnumerable<string> enumerable = fieldName == "Item.GenreIds" ? GetEnumerable(mediaType, fieldName)?.OrderBy(x => x) :
                GetEnumerable(mediaType, fieldName)?.SelectMany(x => x.SplitByListSeparator()).Distinct().OrderBy(x => x);

            if (enumerable != null)
            {
                UniqueValuesCache.Add(key, enumerable);

                IDictionary<string, object> uniqueValues = UniqueValues as IDictionary<string, object>;
                uniqueValues[key] = enumerable;
            }

            return enumerable;
        }

        public static string GetFilterKey(MediaType mediaType, string fieldName)
        {
            return String.Format("{0}_{1}", mediaType, fieldName.Replace("Item.", ""));
        }

        private static IEnumerable<string> GetEnumerable(MediaType mediaType, string fieldName)
        {
            switch (fieldName)
            {
                case "Director":
                    return App.Repository.Movies.Select(x => x.Director);

                case "Writer":
                    return App.Repository.Movies.Select(x => x.Writer);

                case "Creator":
                    return App.Repository.TvShows.Select(x => x.Creator);

                case "Network":
                    return App.Repository.TvShows.Select(x => x.Network);                

                case "Starring":
                    return mediaType == MediaType.Movie ? App.Repository.Movies.Select(x => x.Starring) : App.Repository.TvShows.Select(x => x.Starring);

                case "Company":
                    return mediaType == MediaType.Movie ? App.Repository.Movies.Select(x => x.Company) : App.Repository.TvShows.Select(x => x.Company);

                case "Country":
                    return  mediaType == MediaType.Movie ? App.Repository.Movies.Select(x => x.Country) : App.Repository.TvShows.Select(x => x.Country);

                case "Keywords":
                    return mediaType == MediaType.Movie ? App.Repository.Movies.Select(x => x.Keywords) : App.Repository.TvShows.Select(x => x.Keywords);

                case "Item.GenreIds":
                    IEnumerable<Genre> genres = mediaType == MediaType.Movie ? GenreHelper.MovieGenres : GenreHelper.TvShowGenres;
                    return genres.Where(x => x.Id != 0).OfType<GenreHelper.LocalizedGenre>().Select(x => x.DisplayName);
            }

            return null;
        }

        static FilterHelper()
        {
            Messenger.Default.Register(listener, (CultureChangeMessage message) =>
            {
                UniqueValuesCache.Remove("Movie_GenreIds");
                UniqueValuesCache.Remove("Tv_GenreIds");
                PredefinedFilterCache.Remove("Movie_GenreIds");
                PredefinedFilterCache.Remove("Tv_GenreIds");
            });

            App.Repository.Movies.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Remove)
                {
                    List<string> expiredKeys = PredefinedFilterCache.Keys.Where(x => x.StartsWith("Movie_") && !x.EndsWith("GenreIds")).ToList();
                    foreach (string key in expiredKeys)
                    {
                        UniqueValuesCache.Remove(key);
                        PredefinedFilterCache.Remove(key);
                    }
                }
            };

            App.Repository.TvShows.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Remove)
                {
                    List<string> expiredKeys = PredefinedFilterCache.Keys.Where(x => x.StartsWith("Tv_") && !x.EndsWith("GenreIds")).ToList();
                    foreach (string key in expiredKeys)
                    {
                        UniqueValuesCache.Remove(key);
                        PredefinedFilterCache.Remove(key);
                    }
                }
            };
        }

        private static object listener = new object();

        private static Dictionary<string, IEnumerable<string>> UniqueValuesCache = new Dictionary<string, IEnumerable<string>>();

        private static Dictionary<string, PredefinedFilterCollection> PredefinedFilterCache = new Dictionary<string, PredefinedFilterCollection>();
    }
}
