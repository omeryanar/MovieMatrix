using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using DevExpress.Data.Filtering;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Navigation;
using DevExpress.Xpf.Navigation.Internal;
using MovieMatrix.Helper;
using MovieStore;
using MovieStore.Container;
using TMDbLib.Objects.General;

namespace MovieMatrix.Resources
{
    public class RatingConverter : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double rating = System.Convert.ToDouble(value);
            if (rating == 10)
                return "10";
            else
                return String.Format("{0:0.0}", rating);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class RatingToCriteriaOperatorConverter : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            BinaryOperator binaryOperator = value as BinaryOperator;
            if (ReferenceEquals(binaryOperator, null))
                return null;

            OperandValue rightOperand = binaryOperator.RightOperand as OperandValue;
            return rightOperand.Value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new BinaryOperator(parameter.ToString(), Math.Round((double)(value ?? 0d), 1), BinaryOperatorType.GreaterOrEqual);
        }
    }

    public class RatingToDecimalConverter : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Math.Round((double)(value ?? 0d), 1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Math.Round((double)(value ?? 0d), 1);
        }
    }

    public class RuntimeConverter : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case int runtime:
                    return GetDisplayText(runtime);
                case List<int> runtimeList:
                    return String.Join(" - ", runtimeList.Select(x => GetDisplayText(x)));
            }

            return String.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        private string GetDisplayText(int runtime)
        {
            if (runtime > 0)
            {
                int hours = runtime / 60;
                int minutes = runtime % 60;

                if (hours > 0 && minutes == 0)
                    return String.Format("{0} {1}", hours, Properties.Resources.Hours);
                else if (hours == 0 && minutes > 0)
                    return String.Format("{0} {1}", minutes, Properties.Resources.Minutes);
                else if (hours > 0 && minutes > 0)
                    return String.Format("{0} {1} {2} {3}", hours, Properties.Resources.Hours, minutes, Properties.Resources.Minutes);
            }

            return String.Empty;
        }
    }

    public class LanguageConverter : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return LanguageHelper.Languages.FirstOrDefault(x => String.CompareOrdinal(x.TwoLetterISOLanguageName, value?.ToString()) == 0)?.EnglishName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ResourceStringConverter : MarkupExtension, IValueConverter, IMultiValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
                return GetResourceString(value.ToString());
            else if (parameter != null)
                return GetResourceString(parameter.ToString());
            else
                return String.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length > 0)
                return Convert(values[0], targetType, parameter, culture);

            return String.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }

        public string GetResourceString(string value)
        {
            try
            {
                if (value.Contains(Utilities.ListSeparator))
                    return value.SplitByListSeparator().Select(x => GetResourceString(x)).Join();
                else
                    return Properties.Resources.ResourceManager.GetString(value.Replace(" ", ""), Properties.Resources.Culture);
            }
            catch
            {
                return value;
            }
        }
    }

    public class GenreIdToStringConverter : MarkupExtension, IValueConverter, IMultiValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                if (value is int genreId)
                    return GetGenreName(genreId);
                else if (value is IEnumerable<int> genreIds)
                    return String.Join(Utilities.ListSeparator, genreIds.Select(x => GetGenreName(x)));
            }

            return String.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length > 0)
                return Convert(values[0], targetType, parameter, culture);

            return String.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }

        private string GetGenreName(int genreId)
        {
            Genre genre = GenreHelper.MovieGenres.FirstOrDefault(x => x.Id == genreId);
            if (genre == null)
                genre = GenreHelper.TvShowGenres.FirstOrDefault(x => x.Id == genreId);

            return genre != null ? Properties.Resources.ResourceManager.GetString(genre.Name, Properties.Resources.Culture) : String.Empty;
        }
    }

    public class MediaTranslationConverter : MarkupExtension, IValueConverter, IMultiValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return GetTranslation(value, parameter?.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length > 0)
                return Convert(values[0], targetType, parameter, culture);

            return String.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }

        private string GetTranslation(object value, string parameter)
        {
            switch (value)
            {
                case MovieContainer movie:
                    if (parameter == "Title")
                        return movie.GetTitle(Properties.Settings.Default.Language);
                    else if (parameter == "Overview")
                        return movie.GetOverview(Properties.Settings.Default.Language);
                    break;

                case PersonContainer person:
                    if (parameter == "Biography")
                        return person.GetBiography(Properties.Settings.Default.Language);
                    break;

                case TvShowContainer tvShow:
                    if (parameter == "Name")
                        return tvShow.GetName(Properties.Settings.Default.Language);
                    else if (parameter == "Overview")
                        return tvShow.GetOverview(Properties.Settings.Default.Language);
                    break;
            }

            return String.Empty;
        }
    }

    public class ObjectToVisibilityConverter : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class CollectionToVisibilityConverter : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ICollection collection)
                return collection?.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class TileNavPaneAlignmentConverter : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TileNavPaneBar pane = value as TileNavPaneBar;
            BindingExpression bindingExpression = pane.GetBindingExpression(ItemsControl.ItemsSourceProperty);
            if (bindingExpression != null)
            {
                NavButton navButton = bindingExpression.DataItem as NavButton;
                if (navButton?.HorizontalAlignment == HorizontalAlignment.Right)
                    return HorizontalAlignment.Right;
            }
            return HorizontalAlignment.Left;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class FontToGlyphConverter : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double emSize = 24;
            if (parameter is double size)
                emSize = size;

            return CreateImageSource(value.ToString(), emSize);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        private static ImageSource CreateImageSource(string character, double emSize = 24)
        {
            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext drawingContext = visual.RenderOpen())
            {
                Brush brush = new SolidColorBrush(Colors.White);
                drawingContext.DrawText(new FormattedText(character, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, FontTypeface, emSize, brush)
                { TextAlignment = TextAlignment.Center }, new Point(0, 0));
            }

            return new DrawingImage(visual.Drawing);
        }

        private static readonly FontFamily FontFamily = new FontFamily(new Uri("pack://application:,,,"), "./Assets/Font/#Material Design Icons");

        private static readonly Typeface FontTypeface = new Typeface(FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
    }

    public class BirthDateToSignConverter : MarkupExtension, IValueConverter, IMultiValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime birthDate)
                return GetSign(birthDate);

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length > 0)
                return Convert(values[0], targetType, parameter, culture);

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }

        private string GetSign(DateTime birthDate)
        {
            switch (birthDate.Month)
            {
                case 1: return birthDate.Day < 20 ? Properties.Resources.Capricorn : Properties.Resources.Aquarius;
                case 2: return birthDate.Day < 19 ? Properties.Resources.Aquarius : Properties.Resources.Pisces;
                case 3: return birthDate.Day < 21 ? Properties.Resources.Pisces : Properties.Resources.Aries;
                case 4: return birthDate.Day < 20 ? Properties.Resources.Aries : Properties.Resources.Taurus;
                case 5: return birthDate.Day < 21 ? Properties.Resources.Taurus : Properties.Resources.Gemini;
                case 6: return birthDate.Day < 21 ? Properties.Resources.Gemini : Properties.Resources.Cancer;
                case 7: return birthDate.Day < 23 ? Properties.Resources.Cancer : Properties.Resources.Leo;
                case 8: return birthDate.Day < 23 ? Properties.Resources.Leo : Properties.Resources.Virgo;
                case 9: return birthDate.Day < 23 ? Properties.Resources.Virgo : Properties.Resources.Libra;
                case 10: return birthDate.Day < 23 ? Properties.Resources.Libra : Properties.Resources.Scorpio;
                case 11: return birthDate.Day < 22 ? Properties.Resources.Scorpio : Properties.Resources.Sagittarius;
                case 12: return birthDate.Day < 22 ? Properties.Resources.Sagittarius : Properties.Resources.Capricorn;
                default: return String.Empty;
            }
        }
    }

    public class PathToFileListConverter : MarkupExtension, IValueConverter
    {
        public bool AllowFolders { get; set; } = true;

        public string[] FileFilter { get; set; } = FileHelper.VideoFiles;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string rootPath = value?.ToString();
            if (String.IsNullOrEmpty(rootPath) || !Directory.Exists(rootPath))
                return null;

            List<FileModel> items = new List<FileModel>();
            if (AllowFolders)
                items.Add(new FileModel(new DirectoryInfo(rootPath)));

            string[] fileSystemEntries = Directory.GetFileSystemEntries(rootPath, "*.*", SearchOption.AllDirectories);

            foreach (string path in fileSystemEntries)
            {
                FileModel fileModel = null;
                if (FileFilter.Any(x => path.EndsWith(x, StringComparison.OrdinalIgnoreCase)))
                    fileModel = new FileModel(new FileInfo(path));
                else if (AllowFolders && Directory.Exists(path))
                    fileModel = new FileModel(new DirectoryInfo(path));

                if (fileModel != null)
                    items.Add(fileModel);
            }

            return items;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        public class FileModel
        {
            public string Name { get; set; }

            public string FullPath { get; set; }

            public string ParentName { get; set; }

            public string ParentPath { get; set; }

            public FileModel(FileInfo fileInfo)
            {
                Name = fileInfo.Name;
                ParentName = fileInfo.Directory.Name;

                FullPath = RemoveTrailingSeparator(fileInfo.FullName);
                ParentPath = RemoveTrailingSeparator(fileInfo.Directory.FullName);
            }

            public FileModel(DirectoryInfo directoryInfo)
            {
                Name = directoryInfo.Name;
                ParentName = directoryInfo.Parent.Name;

                FullPath = RemoveTrailingSeparator(directoryInfo.FullName);
                ParentPath = RemoveTrailingSeparator(directoryInfo.Parent.FullName);
            }

            public override string ToString()
            {
                return FullPath;
            }

            private string RemoveTrailingSeparator(string path)
            {
                return path?.EndsWith("\\") == true ? path.Substring(0, path.Length - 1) : path;
            }
        }
    }
}
