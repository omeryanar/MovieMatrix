using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using DevExpress.Mvvm;
using TMDbLib.Objects.General;

namespace MovieMatrix.Helper
{
    public class FileHelper
    {
        public static readonly string[] ImageFiles =
        { 
            ".BMP",
            ".PNG",
            ".JPG",
            ".JPEG",
            ".GIF",
            ".SVG",
            ".TIF",
            ".TIFF"
        };

        public static readonly string[] VideoFiles =
        {
            ".AVI",
            ".DIVX",
            ".FLV",
            ".M4V",
            ".MKV",
            ".MOV",
            ".MP4",
            ".MPEG",
            ".MPG",
            ".RM",
            ".RMVB",
            ".VOB",
            ".XVID",
            ".WMV"
        };
    }

    public class GenreHelper
    {
        public static ReadOnlyCollection<Genre> MovieGenres { get; } = new ReadOnlyCollection<Genre>(new[]
        {            
            new LocalizedGenre { Id = 28, Name = "Action" },
            new LocalizedGenre { Id = 12, Name = "Adventure" },
            new LocalizedGenre { Id = 16, Name = "Animation" },
            new LocalizedGenre { Id = 35, Name = "Comedy" },
            new LocalizedGenre { Id = 80, Name = "Crime" },
            new LocalizedGenre { Id = 99, Name = "Documentary" },
            new LocalizedGenre { Id = 18, Name = "Drama" },
            new LocalizedGenre { Id = 10751, Name = "Family" },
            new LocalizedGenre { Id = 14, Name = "Fantasy" },
            new LocalizedGenre { Id = 36, Name = "History" },
            new LocalizedGenre { Id = 27, Name = "Horror" },
            new LocalizedGenre { Id = 10402, Name = "Music" },
            new LocalizedGenre { Id = 9648, Name = "Mystery" },
            new LocalizedGenre { Id = 10749, Name = "Romance" },
            new LocalizedGenre { Id = 878, Name = "Science_Fiction" },
            new LocalizedGenre { Id = 10770, Name = "TV_Movie" },
            new LocalizedGenre { Id = 53, Name = "Thriller" },
            new LocalizedGenre { Id = 10752, Name = "War" },
            new LocalizedGenre { Id = 37, Name = "Western" },
            new LocalizedGenre { Id = 0, Name = "AllGenres" }
        });

        public static ReadOnlyCollection<Genre> TvShowGenres { get; } = new ReadOnlyCollection<Genre>(new[]
        {            
            new LocalizedGenre { Id = 10759, Name = "Action_Adventure" },
            new LocalizedGenre { Id = 16, Name = "Animation" },
            new LocalizedGenre { Id = 35, Name = "Comedy" },
            new LocalizedGenre { Id = 80, Name = "Crime" },
            new LocalizedGenre { Id = 99, Name = "Documentary" },
            new LocalizedGenre { Id = 18, Name = "Drama" },
            new LocalizedGenre { Id = 10751, Name = "Family" },
            new LocalizedGenre { Id = 10762, Name = "Kids" },
            new LocalizedGenre { Id = 9648, Name = "Mystery" },
            new LocalizedGenre { Id = 10763, Name = "News" },
            new LocalizedGenre { Id = 10764, Name = "Reality" },
            new LocalizedGenre { Id = 10765, Name = "Sci_Fi_Fantasy" },
            new LocalizedGenre { Id = 10766, Name = "Soap" },
            new LocalizedGenre { Id = 10767, Name = "Talk" },
            new LocalizedGenre { Id = 10768, Name = "War_Politics" },
            new LocalizedGenre { Id = 37, Name = "Western" },
            new LocalizedGenre { Id = 0, Name = "AllGenres" }
        });

        public class LocalizedGenre : Genre, INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public string DisplayName
            {
                get { return Properties.Resources.ResourceManager.GetString(Name, Properties.Resources.Culture); }
            }

            public LocalizedGenre()
            {
                Messenger.Default.Register(this, (CultureChangeMessage message) => 
                { 
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayName)));
                });
            }
        }
    }

    public class CertificationHelper
    {
        public static ReadOnlyCollection<Certification> MovieCertifications { get; } = new ReadOnlyCollection<Certification>(new[]
        {
            new Certification("US", "United States", "NR"),
            new Certification("US", "United States", "G"),
            new Certification("US", "United States", "PG"),
            new Certification("US", "United States", "PG-13"),
            new Certification("US", "United States", "R"),
            new Certification("US", "United States", "NC-17"),
            new Certification("CA", "Canada", "G"),
            new Certification("CA", "Canada", "PG"),
            new Certification("CA", "Canada", "14A"),
            new Certification("CA", "Canada", "18A"),
            new Certification("CA", "Canada", "A"),
            new Certification("AU", "Australia", "E"),
            new Certification("AU", "Australia", "G"),
            new Certification("AU", "Australia", "PG"),
            new Certification("AU", "Australia", "M"),
            new Certification("AU", "Australia", "MA15+"),
            new Certification("AU", "Australia", "R18+"),
            new Certification("AU", "Australia", "X18+"),
            new Certification("DE", "Germany", "0"),
            new Certification("DE", "Germany", "6"),
            new Certification("DE", "Germany", "12"),
            new Certification("DE", "Germany", "16"),
            new Certification("DE", "Germany", "18"),
            new Certification("FR", "France", "U"),
            new Certification("FR", "France", "10"),
            new Certification("FR", "France", "12"),
            new Certification("FR", "France", "16"),
            new Certification("FR", "France", "18"),
            new Certification("GB", "United Kingdom", "U"),
            new Certification("GB", "United Kingdom", "PG"),
            new Certification("GB", "United Kingdom", "12A"),
            new Certification("GB", "United Kingdom", "12"),
            new Certification("GB", "United Kingdom", "15"),
            new Certification("GB", "United Kingdom", "18"),
            new Certification("GB", "United Kingdom", "R18")
        });

        public static ReadOnlyCollection<Certification> TvShowCertifications { get; } = new ReadOnlyCollection<Certification>(new[]
        {
            new Certification("USA", "United States", "NR"),
            new Certification("USA", "United States", "TV-Y"),
            new Certification("USA", "United States", "TV-Y7"),
            new Certification("USA", "United States", "TV-G"),
            new Certification("USA", "United States", "TV-PG"),
            new Certification("USA", "United States", "TV-14"),
            new Certification("USA", "United States", "TV-MA"),
            new Certification("CA", "Canada", "C"),
            new Certification("CA", "Canada", "C8"),
            new Certification("CA", "Canada", "G"),
            new Certification("CA", "Canada", "PG"),
            new Certification("CA", "Canada", "14+"),
            new Certification("CA", "Canada", "18+"),
            new Certification("AU", "Australia", "P"),
            new Certification("AU", "Australia", "C"),
            new Certification("AU", "Australia", "G"),
            new Certification("AU", "Australia", "PG"),
            new Certification("AU", "Australia", "M"),
            new Certification("AU", "Australia", "MA15+"),
            new Certification("AU", "Australia", "AV15+"),
            new Certification("AU", "Australia", "R18+"),
            new Certification("DE", "Germany", "0"),
            new Certification("DE", "Germany", "6"),
            new Certification("DE", "Germany", "12"),
            new Certification("DE", "Germany", "16"),
            new Certification("DE", "Germany", "18"),
            new Certification("FR", "France", "NR"),
            new Certification("FR", "France", "10"),
            new Certification("FR", "France", "12"),
            new Certification("FR", "France", "16"),
            new Certification("FR", "France", "18"),
            new Certification("GB", "England", "U"),
            new Certification("GB", "England", "PG"),
            new Certification("GB", "England", "12A"),
            new Certification("GB", "England", "12"),
            new Certification("GB", "England", "15"),
            new Certification("GB", "England", "18"),
            new Certification("GB", "England", "R18")
        });

        public class Certification
        {
            public Certification(string countryCode, string countryName, string certificationCode)
            {
                CountryCode = countryCode;
                CountryName = countryName;
                CertificationCode = certificationCode;
            }

            public string CountryName { get; private set; }

            public string CountryCode { get; private set; }

            public string CertificationCode { get; private set; }

            public override string ToString()
            {
                return String.Format("{0} - {1}", CountryName, CertificationCode);
            }
        }
    }

    public class LanguageHelper
    {
        public static CultureInfo[] Languages { get; } = CultureInfo.GetCultures(CultureTypes.NeutralCultures).
            Where(x => x.TwoLetterISOLanguageName != "iv" && x.TwoLetterISOLanguageName.Length == 2).OrderBy(x => x.EnglishName).GroupBy(x => x.TwoLetterISOLanguageName).Select(x => x.First()).ToArray();
    }
}
