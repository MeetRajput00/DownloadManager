using DownloadManager.Models.Models;
using DownloadManager.Services.IServices;
using DownloadManager.Services.Services;
using Newtonsoft.Json;

namespace DownloadManager.Services
{
    public static class Lists
    {
        private const string systemDataFileName = "systemData.json";

        private static readonly IStorageService _storageService = new StorageService();

        public static DownloadConfiguration _downloadedConfiguration { get; set; } = new DownloadConfiguration();

        public static List<DownloadItem> _downloadedItems { get; set; }

        public static async Task InitializeAsync()
        {
            try
            {
                var systemData = JsonConvert.DeserializeObject<SystemData>(await _storageService.GetValueAsync(systemDataFileName));
                SetSystemData(systemData);
            }
            catch (Exception ex)
            {
                _downloadedItems = new List<DownloadItem>();
            }
        }

        public static async Task UpdateAsync()
        {
            var systemData = new SystemData
            {
                DownloadItems = _downloadedItems
            };
            try
            {
                await _storageService.SetValueAsync(systemDataFileName, JsonConvert.SerializeObject(systemData));
            }
            catch (Exception ex)
            {
            }
        }

        private static void SetSystemData(SystemData? systemData)
        {
            _downloadedItems = systemData?.DownloadItems ?? new List<DownloadItem>();
        }
    }
}
