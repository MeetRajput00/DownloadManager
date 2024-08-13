using DownloadManager.Services.IServices;

namespace DownloadManager.Services.Services
{
    public class StorageService : IStorageService, IFileService
    {
        public async Task<string> GetValueAsync(string path)
        {
            var fileName = System.IO.Path.Combine(FileSystem.CacheDirectory, path);
            return File.ReadAllText(fileName);
        }

        public async Task<string> SaveFile(MemoryStream stream, string path, CancellationToken cancellationToken)
        {
            var fileName = System.IO.Path.Combine(FileSystem.CacheDirectory, path);
            File.WriteAllBytes(fileName, stream.ToArray());
            MainThread.BeginInvokeOnMainThread(NotificationService.ShowFileSavedSuccessfully);
            return fileName;
        }

        public async Task SetValueAsync(string path, string value)
        {
            var fileName = System.IO.Path.Combine(FileSystem.CacheDirectory, path);
            File.WriteAllText(fileName, value);
        }
    }
}
