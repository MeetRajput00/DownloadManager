using CommunityToolkit.Mvvm.Input;
using DownloadManager.Models.Models;
using DownloadManager.Services;
using DownloadManager.Services.IServices;
using DownloadManager.Services.Services;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace DownloadManager.ViewModels
{
    public class DownloadPageViewModel : ViewModelBase
    {
        private readonly IDownloadService _downloadService;

        private readonly IFileService _fileService;

        private DownloadItem _currentDownloadItem = new DownloadItem();

        private ObservableCollection<DownloadItem> _downloadedItems = new ObservableCollection<DownloadItem>(Lists._downloadedItems);

        private string _downloadUrl;

        private bool _isValidUrl;

        public DownloadItem CurrentDownloadItem
        {
            get => _currentDownloadItem;
            set => SetProperty(ref _currentDownloadItem, value);
        }

        public ICommand DownloadCommand => new AsyncRelayCommand(async () =>
        {
            await DownloadCommandExecute();
            RaisePropertyChanged(nameof(ListViewHeader));
            await UpdateCache();
        });

        public ObservableCollection<DownloadItem> DownloadedItems
        {
            get => _downloadedItems;
            set => SetProperty(ref _downloadedItems, value);
        }

        public string DownloadUrl
        {
            get { return _downloadUrl; }
            set { SetPropertyChanged(ref _downloadUrl, value); }
        }

        public bool IsValidUrl
        {
            get
            {
                string urlPattern = @"^(http|https)://[^\s/$.?#].[^\s]*$";
                return Regex.IsMatch(DownloadUrl, urlPattern, RegexOptions.IgnoreCase);
            }
        }

        public string ListViewHeader => $"Downloaded Items ({DownloadedItems.Count})";

        public DownloadPageViewModel()
        {
            _fileService = new StorageService();
            _downloadService = new DownloadService();
        }

        private async Task DownloadCommandExecute()
        {
            if (!IsValidUrl)
            {
                MainThread.BeginInvokeOnMainThread(NotificationService.ShowInvalidUrlPopUp);
                return;
            }
            var uri = new Uri(DownloadUrl);
            var filename = System.IO.Path.GetFileName(uri.LocalPath);
            try
            {
                CurrentDownloadItem = new DownloadItem()
                {
                    StartTime = DateTime.Now,
                    Url = DownloadUrl,
                    Title = filename,
                    Status = Models.Enums.DownloadStatus.Downloading
                };
                DownloadedItems.Insert(0, CurrentDownloadItem);
                _downloadService.SetCurrentItem(CurrentDownloadItem);
                var memoryStream = await _downloadService.Download(DownloadUrl);
                CurrentDownloadItem.FilePath = await _fileService.SaveFile(memoryStream, filename, new CancellationToken());
                CurrentDownloadItem.Status = Models.Enums.DownloadStatus.Finished;
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(NotificationService.ShowInvalidUrlPopUp);
                CurrentDownloadItem.Status = Models.Enums.DownloadStatus.Error;
            }
        }

        private async Task UpdateCache()
        {
            Lists._downloadedItems = DownloadedItems.ToList();
            await Lists.UpdateAsync();
        }
    }
}
