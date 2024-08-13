using DownloadManager.Models.Models;
using DownloadManager.Services.IServices;

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
            int numberOfSegments = 50;
            long segmentSize = fileSize / numberOfSegments;
            var tasks = new List<Task<byte[]>>();
            for (int i = 0; i < numberOfSegments; i++)
            {
                long start = i * segmentSize;
                long end = (i == numberOfSegments - 1) ? fileSize - 1 : (start + segmentSize - 1);

                tasks.Add(DownloadSegment(url, start, end).ContinueWith<byte[]>(segmentTask =>
                {
                    if (segmentTask.Status == TaskStatus.RanToCompletion && segmentTask.Result != null)
                    {
                        var segment = segmentTask.Result;
                        _currentlyDownloadingItem.BytesDownloaded += segment.Length;
                    }
                    return segmentTask.Result ?? [];
                }));
            }
            var memoryStream = new MemoryStream();
            try
            {
                byte[][] segments = await Task.WhenAll(tasks);
                foreach (var segment in segments)
                {
                    memoryStream.Write(segment, 0, segment.Length);
                }
                memoryStream.Position = 0;
            }
            catch (Exception ex)
            {
            }
            return memoryStream;
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

        private async Task<byte[]> DownloadSegment(string url, long start, long end)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(start, end);

                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
            }
            return null;
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
