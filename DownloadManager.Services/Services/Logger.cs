using DownloadManager.Services.IServices;
using System.Collections.Concurrent;

namespace DownloadManager.Services.Services
{
    public class Logger : ILogger, IDisposable
    {
        protected readonly ConcurrentQueue<string> LogQueue;

        protected string LogPath = Path.Combine(FileSystem.CacheDirectory, "logs_" + DateTime.Now.ToString("yyyyMMdd.HHmmss") + ".log");

        protected StreamWriter LogStream;

        private volatile bool _disposed;

        private SemaphoreSlim _semaphore;

        public Logger()
        {
            _semaphore = new SemaphoreSlim(0);
            LogQueue = new ConcurrentQueue<string>();
            LogStream = new StreamWriter(CreateFile(LogPath));

            Task<Task> task = Task.Factory.StartNew(
                    function: Writer,
                    cancellationToken: default,
                    creationOptions: TaskCreationOptions.LongRunning,
                    scheduler: TaskScheduler.Default);

            task.Unwrap();
        }

        public void Dispose()
        {
            _disposed = true;
            LogStream?.Dispose();
            LogStream = null;
        }

        public async Task FlushAsync()
        {
            while (!_disposed && _semaphore.CurrentCount > 0)
            {
                await Task.Delay(100);
            }

            await (LogStream?.FlushAsync() ?? Task.FromResult(0)).ConfigureAwait(false);
        }

        public virtual string Formatter(string logType, string message, Exception exception)
        {
            var log = $"{DateTime.Now:s} | {logType} | {message}";
            if (exception is not null)
            {
                log += " | " + exception.Message + ": " + exception.StackTrace;
            }

            return log;
        }

        public void LogCritical(string message)
        {
            Log(nameof(LogCritical), message);
        }

        public void LogCritical(Exception exception, string message)
        {
            Log(nameof(LogCritical), message, exception);
        }

        public void LogDebug(string message)
        {
            Log(nameof(LogDebug), message);
        }

        public void LogError(string message)
        {
            Log(nameof(LogError), message);
        }

        public void LogError(Exception exception, string message)
        {
            Log(nameof(LogError), message, exception);
        }

        public void LogInfo(string message)
        {
            Log(nameof(LogInfo), message);
        }

        public void LogWarning(string message)
        {
            Log(nameof(LogWarning), message);
        }

        protected void Log(string logType, string message, Exception exception = null)
        {
            if (!_disposed)
            {
                LogQueue.Enqueue(Formatter(logType, message, exception));
                _semaphore.Release();
            }
        }

        private static Stream CreateFile(string filename)
        {
            string directory = Path.GetDirectoryName(filename);
            if (string.IsNullOrWhiteSpace(directory))
            {
                return Stream.Null;
            }

            if (Directory.Exists(directory) == false)
            {
                Directory.CreateDirectory(directory);
            }

            return new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);
        }

        private async Task Writer()
        {
            while (!_disposed)
            {
                await _semaphore.WaitAsync().ConfigureAwait(false);
                if (LogQueue.TryDequeue(out var log))
                {
                    await LogStream.WriteLineAsync(log).ConfigureAwait(false);
                }
            }
        }
    }
}
