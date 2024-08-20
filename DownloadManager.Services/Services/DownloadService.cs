using DownloadManager.Models.Models;
using DownloadManager.Services.IServices;
using System.Net.Http.Headers;

namespace DownloadManager.Services.Services
{
    public class DownloadService : IDownloadService
    {
        private readonly HttpClient _httpClient;

        private DownloadItem _currentlyDownloadingItem { get; set; }

        public DownloadService() => _httpClient = new HttpClient();

        public async Task<MemoryStream> Download(string url)
        {
            if (!await IsSegmentedDownloadSupported(url))
            {
                throw new NotSupportedException();
            }
            long fileSize = await GetFileSize(url);
            _currentlyDownloadingItem.TotalBytes = fileSize;
            int maxDegreeOfParallelism = 6;
            long segmentSize = fileSize / maxDegreeOfParallelism;
            var tasks = new List<Task>();
            var memoryStream = new MemoryStream();
            var semaphore = new SemaphoreSlim(maxDegreeOfParallelism);
            var progressLock = new object();
            for (int i = 0; i < maxDegreeOfParallelism; i++)
            {
                await semaphore.WaitAsync();
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
                    try
                    {
                        memoryStream.Position = start;
                        await DownloadSegment(url, start, end, memoryStream, progress, CancellationToken.None);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }
            await Task.WhenAll(tasks);
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

        public async Task<bool> IsSegmentedDownloadSupported(string url)
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

        public void SetCurrentItem(DownloadItem currentlyDownloadingItem)
        {
            _currentlyDownloadingItem = currentlyDownloadingItem;
        }

        private async Task<byte[]> DownloadFileAsync(string url)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsByteArrayAsync();
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
    }
}
