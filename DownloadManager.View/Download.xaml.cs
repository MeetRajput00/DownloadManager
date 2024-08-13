using DownloadManager.Models.Models;
using DownloadManager.ViewModels;
using UraniumUI.Material.Controls;

namespace DownloadManager.View;

public partial class Download : ContentPage
{
    public DownloadPageViewModel ViewModel { get; set; }

    public Download()
    {
        InitializeComponent();
        ViewModel = new DownloadPageViewModel();
        BindingContext = ViewModel;
    }

    private void Download_url_text_changed(object sender, TextChangedEventArgs e)
    {
        var textField = sender as TextField;
        if (textField == null)
            return;
        textField.BorderColor = (ViewModel.IsValidUrl) ? Colors.Green : Colors.Red;
    }

    private async void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        var item = (sender as ListView).SelectedItem as DownloadItem;
        var test = await Launcher.Default.OpenAsync(new OpenFileRequest("Open", new ReadOnlyFile(item.FilePath)));
    }
}