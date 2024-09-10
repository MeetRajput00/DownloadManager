using DownloadManager.Models.Models;
using DownloadManager.ViewModels;
using UraniumUI.Material.Controls;

namespace DownloadManager.View;

public partial class Download : ContentPage, IQueryAttributable
{
    public DownloadPageViewModel ViewModel { get; set; }

    public Download(DownloadPageViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        BindingContext = ViewModel;
    }

    // Implement IQueryAttributable to handle query parameters
    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("myString"))
        {
            var downloadUrl = Uri.UnescapeDataString(query["myString"] as string);
            ViewModel.DownloadUrl = downloadUrl;
            ViewModel.DownloadCommand.Execute(null);
        }
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