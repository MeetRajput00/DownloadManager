using DownloadManager.Models.Enums;

namespace DownloadManager.Models.Models
{
    public partial class DownloadItem
    {
        public string? Description { get; set; }

        public DateTime? EndTime { get; set; }

        public string? FilePath { get; set; }

        public DateTime StartTime { get; set; }

        public DownloadStatus? Status { get; set; }

        public string? Title { get; set; }

        public string? Url { get; set; }

        private int byteCount { get; set; }
    }
}
