using DownloadManager.Services.IServices;

namespace DownloadManager.Services.Services
{
    public class StorageService : IStorageService, IFileService, IDisposable
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public void Dispose()
        {
            _semaphore?.Dispose();
        }

        public async Task<string> GetValueAsync(string path)
        {
            try
            {
                await _semaphore.WaitAsync();
                var fileName = System.IO.Path.Combine(FileSystem.CacheDirectory, path);
                return File.ReadAllText(fileName);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<string> SaveFile(MemoryStream stream, string path, CancellationToken cancellationToken)
        {
            try
            {
                await _semaphore.WaitAsync(cancellationToken);
                var fileName = System.IO.Path.Combine(FileSystem.CacheDirectory, path);
                using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await stream.CopyToAsync(fileStream, bufferSize: 81920, cancellationToken);
                }
                MainThread.BeginInvokeOnMainThread(NotificationService.ShowFileSavedSuccessfully);
                return fileName;
            }
            finally
            {
                stream?.Dispose();
                _semaphore.Release();
            }
        }

        public async Task SetValueAsync(string path, string value)
        {
            try
            {
                await _semaphore.WaitAsync();
                var fileName = System.IO.Path.Combine(FileSystem.CacheDirectory, path);
                File.WriteAllText(fileName, value);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
