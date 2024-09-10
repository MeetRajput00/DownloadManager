using CommunityToolkit.Mvvm.Input;
using DownloadManager.Models.Models;
using DownloadManager.Services;
using DownloadManager.Services.IServices;
using DownloadManager.Services.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace DownloadManager.ViewModels
{
    public class DownloadPageViewModel : ViewModelBase
    {
        private readonly IDownloadService _downloadService;

        private readonly IFileService _fileService;

        private readonly ILogger _logger;

        private readonly IMapper _mapper;

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
            if (!IsValidUrl)
            {
                MainThread.BeginInvokeOnMainThread(NotificationService.ShowInvalidUrlPopUp);
                return;
            }
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
                return string.IsNullOrEmpty(DownloadUrl) ? false : Regex.IsMatch(DownloadUrl, urlPattern, RegexOptions.IgnoreCase);
            }
        }

        public string ListViewHeader => $"Downloaded Items ({DownloadedItems.Count})";

        public DownloadPageViewModel(IFileService fileService,
            IDownloadService downloadService,
            ILogger logger,
            IMapper mapper)
        {
            _fileService = fileService;
            _downloadService = downloadService;
            _logger = logger;
            _mapper = mapper;
        }

        private async Task DownloadCommandExecute()
        {
            CurrentDownloadItem = _mapper.UrlToDownloadItem(DownloadUrl);
            var filename = CurrentDownloadItem.Title;
            try
            {
                DownloadedItems.Insert(0, CurrentDownloadItem);
                _downloadService.SetCurrentItem(CurrentDownloadItem);
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                _logger.LogInfo($"Download started for {filename}.");
                var memoryStream = await _downloadService.Download(DownloadUrl);
                CurrentDownloadItem.FilePath = await _fileService.SaveFile(memoryStream, filename, new CancellationToken());
                stopWatch.Stop();
                _logger.LogInfo($"Download finished for {filename}. Total time taken: {new TimeSpan(stopWatch.ElapsedTicks)}");
                CurrentDownloadItem.Status = Models.Enums.DownloadStatus.Finished;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Download filed for {filename}. Exception:-{ex.Message}");
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
