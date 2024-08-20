using System.ComponentModel;

namespace DownloadManager.Models.Models
{
    public partial class DownloadItem : INotifyPropertyChanged
    {
        private long _bytesDownloaded;

        private long _totalBytes;

        public long BytesDownloaded
        {
            get { return _bytesDownloaded; }
            set { _bytesDownloaded = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(Progress)); RaisePropertyChanged(nameof(ProgressText)); }
        }

        public string FlagStatus
        {
            get
            {
                if (Status == Enums.DownloadStatus.Downloading)
                {
                    return "Yellow";
                }
                if (Status == Enums.DownloadStatus.Finished)
                {
                    return "Green";
                }
                if (Status == Enums.DownloadStatus.Error)
                {
                    return "Red";
                }
                return "Yellow";
            }
        }

        public double Progress => TotalBytes == 0 ? 0 : (double)BytesDownloaded / (double)TotalBytes;

        public string ProgressText => $"{BytesToMB(BytesDownloaded).ToString("0.00")} MB/{BytesToMB(TotalBytes).ToString("0.00")} MB {double.Round(Progress * 100, 2, MidpointRounding.AwayFromZero).ToString("0.00")}%";

        public string ToolTip => Status.ToString();

        public long TotalBytes
        {
            get { return _totalBytes; }
            set { _totalBytes = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(Progress)); RaisePropertyChanged(nameof(ProgressText)); }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        private double BytesToMB(long currBytes) => double.Round(((double)currBytes / (double)Constants.BytesInOneMB), 2, MidpointRounding.AwayFromZero);
    }
}
