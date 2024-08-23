namespace DownloadManager.Services.IServices
{
    public interface ILogger
    {
        Task FlushAsync();

        string Formatter(string logType, string message, Exception exception);

        void LogCritical(string message);

        void LogCritical(Exception exception, string message);

        void LogDebug(string message);

        void LogError(string message);

        void LogError(Exception exception, string message);

        void LogInfo(string message);

        void LogWarning(string message);
    }
}
