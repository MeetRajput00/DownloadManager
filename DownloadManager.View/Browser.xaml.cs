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

    private void Go_To_Url_Button_Clicked(object sender, EventArgs e)
    {
        string url = ViewModel.Url;

        // Validate and navigate to URL
        if (!string.IsNullOrWhiteSpace(url))
        {
            if (!url.StartsWith("http"))
            {
                url = "https://" + url;
            }
            WebView.Source = url;  // Set WebView source to the URL
        }
    }

    private void OnWebViewNavigated(object sender, WebNavigatedEventArgs e)
    {
        ViewModel.IsLoading = false;
    }

    private void OnWebViewNavigating(object sender, WebNavigatingEventArgs e)
    {
        ViewModel.IsLoading = true;
    }
}