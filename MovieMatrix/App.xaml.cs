using System;
using System.Windows;
using System.Windows.Threading;
using DevExpress.Mvvm;
using DevExpress.Xpf.Core;
using MovieMatrix.Properties;
using MovieMatrix.ViewModel;
using MovieStore;

namespace MovieMatrix
{
    public partial class App : Application
    {
        public static Repository Repository { get; private set; }

        public App()
        {
            Repository = new Repository(Settings.Default.DatabaseName);

            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        public void BringToFront()
        {
            if (Current.MainWindow.WindowState == WindowState.Minimized)
                SystemCommands.RestoreWindow(Current.MainWindow);

            Current.MainWindow.Activate();
        }

        #region Exceptions

        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            Journal.WriteError(e.Exception);

            MessageViewModel viewModel = MessageViewModel.FromException(e.Exception);
            Messenger.Default.Send(viewModel);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception)
            {
                Journal.WriteError(exception);

                MessageViewModel viewModel = MessageViewModel.FromException(exception);
                Messenger.Default.Send(viewModel);
            }
        }

        #endregion
    }
}
