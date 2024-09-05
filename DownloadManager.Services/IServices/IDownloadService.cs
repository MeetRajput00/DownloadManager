using DownloadManager.Models.Models;

namespace DownloadManager.Services.IServices
{
    public interface IDownloadService
    {
        Task<MemoryStream> Download(string url);

        void SetCurrentItem(DownloadItem currentlyDownloadingItem);
    }
}
