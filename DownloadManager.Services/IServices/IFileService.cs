namespace DownloadManager.Services.IServices
{
    public interface IFileService
    {
        Task<string> SaveFile(MemoryStream stream, string fileName, CancellationToken cancellationToken);
    }
}
