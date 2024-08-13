namespace DownloadManager.Services.IServices
{
    public interface IStorageService
    {
        Task<string> GetValueAsync(string path);

        Task SetValueAsync(string path, string value);
    }
}
