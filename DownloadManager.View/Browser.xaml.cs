using DownloadManager.ViewModels;
using Newtonsoft.Json;
using System.Text;

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

    private void Back_To_Previous_Button_Clicked(object sender, EventArgs e)
    {
        if (WebView.CanGoBack)
        {
            WebView.GoBack();
        }
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        GetDownloadableLinks();
    }

    private async void GetDownloadableLinks()
    {
        var jsCode = @"
        (function() {
            var links = document.querySelectorAll('a[href]');
            var downloadLinks = [];
            const downloadableFileExtensions = [
                "".pdf"", "".doc"", "".docx"", "".txt"", "".xls"", "".xlsx"", "".ppt"", "".pptx"",
                "".odt"", "".ods"", "".epub"", "".rtf"", "".jpg"", "".jpeg"", "".png"", "".gif"",
                "".bmp"", "".tiff"", "".tif"", "".svg"", "".ico"", "".webp"", "".mp3"", "".wav"",
                "".aac"", "".ogg"", "".flac"", "".m4a"", "".wma"", "".mp4"", "".avi"", "".mkv"",
                "".mov"", "".wmv"", "".flv"", "".webm"", "".3gp"", "".m4v"", "".zip"", "".rar"",
                "".7z"", "".tar"", "".gz"", "".bz2"", "".iso"", "".dmg"", "".exe"", "".msi"", "".apk"",
                "".bat"", "".sh"", "".jar"", "".app"", "".html"", "".htm"", "".css"", "".js"", "".json"",
                "".xml"", "".csv"", "".php"", "".java"", "".cpp"", "".cs"", "".py"", "".rb""
            ];

            function isDownloadableLink(href) {
                return downloadableFileExtensions.some(ext => href.endsWith(ext));
            }
            links.forEach(function(link) {
                var href = link.getAttribute('href');
                if (isDownloadableLink(href)) {
                    downloadLinks.push(href);
                }
            });
            return JSON.stringify(downloadLinks);
        })();
    ";

        var result = await WebView.EvaluateJavaScriptAsync(jsCode);
        if (!string.IsNullOrEmpty(result))
        {
            try
            {
                ViewModel.SetDownloadableLinks(ParseJsonArray(result));
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error parsing JSON: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("No result returned from JavaScript.");
        }
        await Navigation.PushModalAsync(new DownloadLinksDialog(ViewModel.DownloadableLinks.ToList()));
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

    private void Move_Forward_Button_Clicked(object sender, EventArgs e)
    {
        if (WebView.CanGoForward)
        {
            WebView.GoForward();
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

    private List<string> ParseJsonArray(string input)
    {
        var results = new List<string>();
        var currentUrl = new StringBuilder();
        bool insideString = false;

        foreach (char c in input)
        {
            if (c == '\"')
            {
                if (insideString)
                {
                    results.Add(currentUrl.ToString().TrimEnd('\\'));
                    currentUrl.Clear();
                }
                insideString = !insideString;
            }
            else if (insideString)
            {
                currentUrl.Append(c);
            }
        }

        return results;
    }

    private void Reload_Button_Clicked(object sender, EventArgs e)
    {
        WebView.Reload();
    }
}