using System;
using System.Globalization;
using System.Threading;
using System.Windows.Markup;
using DevExpress.Xpf.Core;
using Microsoft.VisualBasic.ApplicationServices;
using MovieMatrix.Properties;
using MovieMatrix.View;
using MovieStore;

namespace MovieMatrix
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            ApplicationThemeHelper.ApplicationThemeName = "VS2017Dark";

            SingleInstanceApp singleInstanceApp = new SingleInstanceApp();
            singleInstanceApp.Run(args);
        }
    }

    public class SingleInstanceApp : WindowsFormsApplicationBase
    {
        private App application;

        public SingleInstanceApp()
        {
            IsSingleInstance = true;
        }

        protected override bool OnStartup(StartupEventArgs e)
        {
            CultureInfo currentCulture = new CultureInfo(Settings.Default.Language);
            Thread.CurrentThread.CurrentCulture = currentCulture;
            Thread.CurrentThread.CurrentUICulture = currentCulture;

            // TODO
            //System.Windows.FrameworkElement.LanguageProperty.OverrideMetadata(typeof(System.Windows.FrameworkElement),
            //    new System.Windows.FrameworkPropertyMetadata(XmlLanguage.GetLanguage(currentCulture.IetfLanguageTag)));

            DXSplashScreen.Show<SplashView>();

            application = new App();            
            application.InitializeComponent();
            application.Activated += (x, y) =>
            {
                if (DXSplashScreen.IsActive)
                    DXSplashScreen.Close();
            };
            application.Run();

            return false;
        }

        protected override void OnShutdown()
        {
            Journal.Shutdown();
            base.OnShutdown();
        }

        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs e)
        {
            application.BringToFront();
        }
    }
}
