using DownloadManager.Models.Models;

namespace DownloadManager.Services.IServices
{
    public interface IMapper
    {
        DownloadItem UrlToDownloadItem(string url);
    }
}
