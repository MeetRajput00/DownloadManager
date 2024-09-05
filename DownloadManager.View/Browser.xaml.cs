using DownloadManager.ViewModels;

namespace DownloadManager.View;

public partial class Browser : ContentPage
{
    public BrowserPageViewModel ViewModel { get; set; }

    public Browser(BrowserPageViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        BindingContext = ViewModel;
    }
}