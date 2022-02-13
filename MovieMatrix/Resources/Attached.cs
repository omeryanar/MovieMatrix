using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Editors;

namespace MovieMatrix.Resources
{
    public static class MarginAssist
    {
        #region Margin

        public static readonly DependencyProperty MarginProperty =
            DependencyProperty.RegisterAttached("Margin", typeof(Thickness), typeof(MarginAssist), new PropertyMetadata(new Thickness(0), MarginPropertyChanged));

        public static Thickness GetMargin(DependencyObject obj)
        {
            return (Thickness)obj.GetValue(MarginProperty);
        }

        public static void SetMargin(DependencyObject obj, Thickness value)
        {
            obj.SetValue(MarginProperty, value);
        }

        #endregion

        #region TopLeftMargin

        public static readonly DependencyProperty TopLeftMarginProperty =
            DependencyProperty.RegisterAttached("TopLeftMargin", typeof(double), typeof(MarginAssist), new PropertyMetadata(0d, MarginPropertyChanged));

        public static double GetTopLeftMargin(DependencyObject obj)
        {
            return (double)obj.GetValue(TopLeftMarginProperty);
        }

        public static void SetTopLeftMargin(DependencyObject obj, double value)
        {
            obj.SetValue(TopLeftMarginProperty, value);
        }

        #endregion

        #region TopRightMargin

        public static readonly DependencyProperty TopRightMarginProperty =
            DependencyProperty.RegisterAttached("TopRightMargin", typeof(double), typeof(MarginAssist), new PropertyMetadata(0d, MarginPropertyChanged));

        public static double GetTopRightMargin(DependencyObject obj)
        {
            return (double)obj.GetValue(TopRightMarginProperty);
        }

        public static void SetTopRightMargin(DependencyObject obj, double value)
        {
            obj.SetValue(TopRightMarginProperty, value);
        }

        #endregion

        #region BottomLeftMargin

        public static readonly DependencyProperty BottomLeftMarginProperty =
            DependencyProperty.RegisterAttached("BottomLeftMargin", typeof(double), typeof(MarginAssist), new PropertyMetadata(0d, MarginPropertyChanged));

        public static double GetBottomLeftMargin(DependencyObject obj)
        {
            return (double)obj.GetValue(BottomLeftMarginProperty);
        }

        public static void SetBottomLeftMargin(DependencyObject obj, double value)
        {
            obj.SetValue(BottomLeftMarginProperty, value);
        }

        #endregion

        #region BottomRightMargin

        public static readonly DependencyProperty BottomRightMarginProperty =
            DependencyProperty.RegisterAttached("BottomRightMargin", typeof(double), typeof(MarginAssist), new PropertyMetadata(0d, MarginPropertyChanged));

        public static double GetBottomRightMargin(DependencyObject obj)
        {
            return (double)obj.GetValue(BottomRightMarginProperty);
        }

        public static void SetBottomRightMargin(DependencyObject obj, double value)
        {
            obj.SetValue(BottomRightMarginProperty, value);
        }

        #endregion

        private static void MarginPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            Panel panel = obj as Panel;
            if (panel != null)
            {
                panel.Loaded += (x, y) =>
                {
                    Thickness margin = new Thickness(0);
                    if (e.Property == MarginProperty)
                        margin = (Thickness)e.NewValue;
                    else if (e.Property == TopLeftMarginProperty)
                        margin = new Thickness((double)e.NewValue, (double)e.NewValue, 0, 0);
                    else if (e.Property == TopRightMarginProperty)
                        margin = new Thickness(0, (double)e.NewValue, (double)e.NewValue, 0);
                    else if (e.Property == BottomLeftMarginProperty)
                        margin = new Thickness((double)e.NewValue, 0, 0, (double)e.NewValue);
                    else if (e.Property == BottomRightMarginProperty)
                        margin = new Thickness(0, 0, (double)e.NewValue, (double)e.NewValue);

                    foreach (var item in panel.Children)
                    {
                        FrameworkElement element = item as FrameworkElement;
                        if (element != null)
                            element.Margin = margin;
                    }
                };
            }
        }
    }

    public static class ImageAssist
    {
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.RegisterAttached("Source", typeof(string), typeof(ImageAssist), new PropertyMetadata(null, SourcePropertyChanged));

        public static string GetSource(DependencyObject obj)
        {
            return (string)obj.GetValue(SourceProperty);
        }

        public static void SetSource(DependencyObject obj, string value)
        {
            obj.SetValue(SourceProperty, value);
        }

        private static async void SourcePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            BitmapSource bitmapImage = await GetImage(e.NewValue?.ToString());
            if (obj is UIElement element && e.NewValue?.ToString().EndsWith(".png") == true)
            {
                if (bitmapImage.Palette != null)
                    element.Effect = new DropShadowEffect();
                else
                    element.Effect = new DropShadowEffect { ShadowDepth = 1, BlurRadius = 1, Color = Color.FromRgb(20, 20, 20) };
            }

            if (obj is Image image)
                image.Source = bitmapImage;
            else if (obj is ImageEdit imageEdit)
                imageEdit.EditValue = bitmapImage;
            else if (obj is ImageBrush imageBrush)
                imageBrush.ImageSource = bitmapImage;
        }

        private static async Task<BitmapImage> GetImage(string path)
        {
            if (path == null || path.Length < 5)
                return null;

            Stream stream = await App.Repository.GetImageAsync(path);
            if (stream != null)
            {
                BitmapImage bitmapImage = new BitmapImage
                {
                    CreateOptions = BitmapCreateOptions.IgnoreColorProfile,
                    CacheOption = BitmapCacheOption.OnLoad
                };
                try
                {
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = stream;                    
                    bitmapImage.EndInit();
                }
                catch (Exception)
                {
                    return null;
                }

                return bitmapImage;
            }

            return null;
        }
    }

    public static class ClearValueAssist
    {
        public static bool GetIsRoot(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsRootProperty);
        }

        public static void SetIsRoot(DependencyObject obj, bool value)
        {
            obj.SetValue(IsRootProperty, value);
        }

        public static readonly DependencyProperty IsRootProperty =
            DependencyProperty.RegisterAttached("IsRoot", typeof(bool), typeof(ClearValueAssist), new PropertyMetadata(false, IsRootPropertyChanged));

        public static void IsRootPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (Convert.ToBoolean(e.NewValue))
                Root = d;
        }

        private static DependencyObject Root;

        public static bool GetIsClearButton(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsClearButtonProperty);
        }

        public static void SetIsClearButton(DependencyObject obj, bool value)
        {
            obj.SetValue(IsClearButtonProperty, value);
        }

        public static readonly DependencyProperty IsClearButtonProperty =
            DependencyProperty.RegisterAttached("IsClearButton", typeof(bool), typeof(ClearValueAssist), new PropertyMetadata(false, IsClearButtonPropertyChanged));

        public static void IsClearButtonPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (Convert.ToBoolean(e.NewValue))
            {
                if (d is Button button)
                {
                    button.Click += (x, y) =>
                    {
                        if (Root != null)
                        {
                            foreach (var item in LayoutTreeHelper.GetLogicalChildren(Root))
                            {
                                if (item is LookUpEditBase lookUpEdit && lookUpEdit.IsTextEditable == false)
                                    lookUpEdit.SelectedIndex = lookUpEdit.AllowNullInput ? -1 : 0;
                                else if (item is BaseEdit edit)
                                    edit.EditValue = edit.AllowNullInput ? null : 0 as object;
                            }
                        }
                    };
                }
            }
        }
    }
}
