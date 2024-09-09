using DownloadManager.Services;
using System.Collections.ObjectModel;

namespace DownloadManager.ViewModels
{
    public class BrowserPageViewModel : ViewModelBase
    {
        private const string _downloadHeader = "Download";

        private ObservableCollection<string> _downloadableLinks = new ObservableCollection<string>();

        private string _downloadableLinksHeader = _downloadHeader + " (" + 0 + ")";

        private string _url = "https://www.google.com";

        public ObservableCollection<string> DownloadableLinks
        {
            get { return _downloadableLinks; }
            set { _downloadableLinks = value; RaisePropertyChanged(); }
        }

        public string DownloadableLinksHeader
        {
            get { return _downloadableLinksHeader; }
            set { _downloadableLinksHeader = value; RaisePropertyChanged(); }
        }

        public string Url
        {
            get { return _url; }
            set { _url = value; RaisePropertyChanged(); }
        }

        public void SetDownloadableLinks(List<string> downloadableLinks)
        {
            DownloadableLinks = new ObservableCollection<string>(downloadableLinks);
            DownloadableLinksHeader = _downloadHeader + " (" + DownloadableLinks.Count + ")";
        }
    }
}
