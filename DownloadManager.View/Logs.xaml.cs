using DownloadManager.ViewModels;

namespace DownloadManager.View;

public partial class Logs : ContentPage
{
    public LogsPageViewModel ViewModel { get; set; }

    public Logs(LogsPageViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        BindingContext = ViewModel;
    }
}