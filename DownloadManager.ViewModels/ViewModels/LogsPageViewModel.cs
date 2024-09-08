using DownloadManager.Services;

namespace DownloadManager.ViewModels
{
    public class LogsPageViewModel : ViewModelBase
    {
        private string _logs;

        public string Logs
        {
            get { return _logs; }
            set { _logs = value; RaisePropertyChanged(); }
        }

        public LogsPageViewModel()
        {
            LoadLogs();
        }

        private void LoadLogs()
        {
            string cacheDir = FileSystem.CacheDirectory;
            var logFiles = Directory.GetFiles(cacheDir, "logs_*.log");
            string allLogs = string.Empty;

            foreach (var logFile in logFiles)
            {
                string logContent = File.ReadAllText(logFile);
                allLogs += $"Log File: {Path.GetFileName(logFile)}\n{logContent}\n\n";
            }

            Logs = allLogs;
        }
    }
}
