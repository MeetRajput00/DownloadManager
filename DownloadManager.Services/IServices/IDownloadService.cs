using DownloadManager.Models.Models;

namespace DownloadManager.Services.IServices
{
    public interface IDownloadService
    {
        Task<MemoryStream> Download(string url, DownloadConfiguration downloadConfiguration);

        void SetCurrentItem(DownloadItem currentlyDownloadingItem);
    }
}
