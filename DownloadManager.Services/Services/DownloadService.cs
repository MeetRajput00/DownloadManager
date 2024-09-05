using DownloadManager.Models.Models;
using DownloadManager.Services.IServices;
using System.Net.Http.Headers;

namespace DownloadManager.Services.Services
{
    public class DownloadService : IDownloadService
    {
        private readonly HttpClient _httpClient;

        private readonly ILogger _logger;

        private DownloadItem _currentlyDownloadingItem { get; set; }

        public DownloadService(ILogger logger)
        {
            _httpClient = new HttpClient();
            _logger = logger;
        }

        public async Task<MemoryStream> Download(string url)
        {
            long fileSize = await GetFileSize(url);
            _currentlyDownloadingItem.TotalBytes = fileSize;
            var progressLock = new object();
            if (!await IsSegmentedDownloadSupported(url))
            {
                var singleSemaphore = new SemaphoreSlim(1);
                await singleSemaphore.WaitAsync();
                try
                {
                    var completeFileStream = new MemoryStream();
                    var progress = new Progress<long>(bytes =>
                    {
                        lock (progressLock)
                        {
                            _currentlyDownloadingItem.BytesDownloaded += bytes;
                        }
                    });
                    await DownloadFile(url, completeFileStream, progress, CancellationToken.None);
                    completeFileStream.Position = 0;
                    return completeFileStream;
                }
                finally
                {
                    singleSemaphore.Release();
                }
            }

            int maxDegreeOfParallelism = Lists._downloadedConfiguration.MaxDegreeOfParallelism;
            long segmentSize = fileSize / maxDegreeOfParallelism;
            var tasks = new List<Task<MemoryStream>>();
            var semaphore = new SemaphoreSlim(maxDegreeOfParallelism);

            for (int i = 0; i < maxDegreeOfParallelism; i++)
            {
                long start = i * segmentSize;
                long end = (i == maxDegreeOfParallelism - 1) ? fileSize - 1 : (start + segmentSize - 1);
                var progress = new Progress<long>(bytes =>
                {
                    lock (progressLock)
                    {
                        _currentlyDownloadingItem.BytesDownloaded += bytes;
                    }
                });

                tasks.Add(Task.Run(async () =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        var segmentStream = new MemoryStream((int)(end - start + 1));
                        await DownloadSegment(url, start, end, segmentStream, progress, CancellationToken.None);
                        return segmentStream;
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }

            var memoryStream = new MemoryStream();
            var segmentStreams = await Task.WhenAll(tasks);

            foreach (var segmentStream in segmentStreams)
            {
                segmentStream.Position = 0;
                await segmentStream.CopyToAsync(memoryStream);
                segmentStream.Dispose();
            }

            memoryStream.Position = 0;
            return memoryStream;
        }

        public async Task DownloadSegment(string url, long start, long end, Stream targetStream, IProgress<long> progress, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Range = new RangeHeaderValue(start, end);

            using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                response.EnsureSuccessStatusCode();
                var totalBytesRead = 0L;
                var buffer = new byte[81920]; // 80 KB buffer
                var contentStream = await response.Content.ReadAsStreamAsync();

                int bytesRead;
                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                {
                    await targetStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                    totalBytesRead += bytesRead;
                    progress?.Report(bytesRead);
                }
            }
        }

        public void SetCurrentItem(DownloadItem currentlyDownloadingItem)
        {
            _currentlyDownloadingItem = currentlyDownloadingItem;
        }

        private async Task DownloadFile(string url, Stream targetStream, IProgress<long> progress, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                response.EnsureSuccessStatusCode();
                var totalBytesRead = 0L;
                var buffer = new byte[81920]; // 80 KB buffer
                var contentStream = await response.Content.ReadAsStreamAsync();

                int bytesRead;
                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                {
                    await targetStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                    totalBytesRead += bytesRead;
                    progress?.Report(bytesRead);
                }
            }
        }

        private async Task<long> GetFileSize(string url)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Head, url);
                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();
                return response.Content.Headers.ContentLength ?? 0;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        private async Task<bool> IsSegmentedDownloadSupported(string url)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Head, url);
                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();
                if (response.Headers.Contains("Accept-Ranges"))
                {
                    var acceptRanges = response.Headers.GetValues("Accept-Ranges");
                    foreach (var value in acceptRanges)
                    {
                        if (value.Equals("bytes", StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            return false;
        }
    }
}
