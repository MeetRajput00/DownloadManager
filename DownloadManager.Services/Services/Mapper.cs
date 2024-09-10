using DownloadManager.Models.Models;
using DownloadManager.Services.IServices;

namespace DownloadManager.Services.Services
{
    public class Mapper : IMapper
    {
        public DownloadItem UrlToDownloadItem(string url)
        {
            var uri = new Uri(url);
            var filename = Path.GetFileName(uri.LocalPath);
            return new DownloadItem()
            {
                StartTime = DateTime.Now,
                Url = url,
                Title = filename,
                Status = Models.Enums.DownloadStatus.Downloading
            };
        }
    }
}
