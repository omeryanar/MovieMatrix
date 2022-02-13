using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using Microsoft.Web.WebView2.Core;

namespace MovieMatrix.View
{
    public partial class WebBrowserView : Window
    {
        public string Uri { get; set; }

        public WebBrowserView()
        {
            InitializeComponent();

            Loaded += WebBrowserView_Loaded;
            Closing += WebBrowserView_Closing;

            WebView.CoreWebView2InitializationCompleted += (s, e) =>
            {
                WebView.CoreWebView2.ContainsFullScreenElementChanged += (x, y) =>
                {
                    WindowState = WebView.CoreWebView2.ContainsFullScreenElement ? WindowState.Maximized : WindowState.Normal;
                };
            };
        }

        public static string GetAvailableBrowserVersionString()
        {
            if (BrowserVersion == null)
            {
                try
                {
                    BrowserVersion = CoreWebView2Environment.GetAvailableBrowserVersionString();
                }
                catch (Exception)
                {
                    BrowserVersion = String.Empty;
                }
            }

            return BrowserVersion;
        }

        private async void WebBrowserView_Loaded(object sender, RoutedEventArgs e)
        {
            if (Environment == null)
            {
                string userDataFolder = Path.Combine(Path.GetTempPath(), AppDomain.CurrentDomain.FriendlyName);
                Environment = await CoreWebView2Environment.CreateAsync(null, userDataFolder, EnvironmentOptions);
            }

            if (!String.IsNullOrEmpty(Uri))
            {
                await WebView.EnsureCoreWebView2Async(Environment);
                WebView.CoreWebView2.Navigate(Uri);
            }
        }

        private void WebBrowserView_Closing(object sender, CancelEventArgs e)
        {
            WebView.CoreWebView2.Navigate("about:blank");
        }

        private static string BrowserVersion;
        private static CoreWebView2Environment Environment;
        private static readonly CoreWebView2EnvironmentOptions EnvironmentOptions = new CoreWebView2EnvironmentOptions("--autoplay-policy=no-user-gesture-required");
    }
}