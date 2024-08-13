using DownloadManager.ViewModels;

namespace DownloadManager.Views;

public partial class DownloadPage : ContentPage
{
    public DownloadPageViewModel ViewModel { get; set; }

    public DownloadPage()
    {
        this.InitializeComponent();
        ViewModel = new DownloadPageViewModel();
        BindingContext = ViewModel;
    }
}