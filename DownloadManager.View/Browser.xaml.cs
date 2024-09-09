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
            links.forEach(function(link) {
                var href = link.getAttribute('href');
                if (href.endsWith('.pdf') || href.endsWith('.zip') || href.endsWith('.jpg')) {
                    downloadLinks.push(href);
                }
            });
            return JSON.stringify(downloadLinks);
        })();
    ";

        var result = await WebView.EvaluateJavaScriptAsync(jsCode);

        // Check if result is valid before attempting to deserialize
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
                    results.Add(currentUrl.ToString());
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