using CommunityToolkit.Maui.Alerts;
using System.Diagnostics;

namespace DownloadManager
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledExceptionException;
            MainPage = new AppShell();
        }

        private void CurrentDomain_UnhandledExceptionException(object sender, UnhandledExceptionEventArgs e)
        {
            Debug.WriteLine($"***** Handling Unhandled Exception *****: {(e.ExceptionObject as Exception).Message}");
            Toast.Make($"{(e.ExceptionObject as Exception).Message}").Show();
            // YourLogger.LogError($"***** Handling Unhandled Exception *****: {e.Exception.Message}");
        }
    }
}
