using DownloadManager.Services;

namespace DownloadManager.ViewModels
{
    public class BrowserPageViewModel : ViewModelBase
    {
        private string _url = "https://www.google.com";

        public string Url
        {
            get { return _url; }
            set { _url = value; RaisePropertyChanged(); }
        }
    }
}
