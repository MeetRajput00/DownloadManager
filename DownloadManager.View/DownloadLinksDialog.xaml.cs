using DownloadManager.Models.Models;
using DownloadManager.Services.Services;
using System.Collections.ObjectModel;

namespace DownloadManager.View;

public partial class DownloadLinksDialog : ContentPage
{
    public DownloadItem CurrentItem { get; set; }

    public ObservableCollection<DownloadItem> DownloadLinks { get; set; }

    public DownloadLinksDialog(List<string> links)
    {
        InitializeComponent();
        var mapper = new Mapper();
        DownloadLinks = new ObservableCollection<DownloadItem>(links.Select(x => mapper.UrlToDownloadItem(x)));
        BindingContext = this;
    }

    private async void CancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private async void DownloadClicked(object sender, EventArgs e)
    {
        if (CurrentItem != null)
        {
            await Shell.Current.GoToAsync($"//home?myString={Uri.EscapeDataString(CurrentItem.Url)}");
        }
    }
}