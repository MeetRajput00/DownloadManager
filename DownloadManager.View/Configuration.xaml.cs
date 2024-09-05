using DownloadManager.ViewModels;

namespace DownloadManager.View;

public partial class Configuration : ContentPage
{
    public ConfigurationPageViewModel ViewModel { get; set; }

    public Configuration(ConfigurationPageViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        BindingContext = ViewModel;
    }
}