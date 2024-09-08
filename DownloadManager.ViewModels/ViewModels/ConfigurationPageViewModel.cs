using CommunityToolkit.Mvvm.Input;
using DownloadManager.Models.Models;
using DownloadManager.Services;
using System.Windows.Input;

namespace DownloadManager.ViewModels
{
    public class ConfigurationPageViewModel : ViewModelBase
    {
        private DownloadConfiguration _downloadConfiguration = Lists._downloadedConfiguration;

        public ICommand ClearCacheCommand => new AsyncRelayCommand(async () => { await Lists.InitializeAsync(); });

        public DownloadConfiguration DownloadConfiguration
        {
            get { return _downloadConfiguration; }
            set { _downloadConfiguration = value; RaisePropertyChanged(); }
        }
    }
}
