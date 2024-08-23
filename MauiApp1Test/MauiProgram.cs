using CommunityToolkit.Maui;
using DownloadManager.Services;
using MauiIcons.Cupertino;
using MauiIcons.Fluent;
using MauiIcons.Material;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using UraniumUI;

namespace DownloadManager
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .UseUraniumUI()
                .UseUraniumUIMaterial()
                .UseMauiCommunityToolkit()
                .UseFluentMauiIcons()
                .UseMaterialMauiIcons()
                .UseCupertinoMauiIcons();

#if DEBUG
            builder.Logging.AddDebug();
#endif
            builder.ConfigureLifecycleEvents(AppLifecycle =>
            {
#if ANDROID
                AppLifecycle.AddAndroid(android =>
                    android.OnCreate(async (activity, bundle) =>
                    {
                        await Lists.InitializeAsync();
                    }));
#endif
            });
            builder.Services.AddSingleton<Services.IServices.ILogger, Services.Services.Logger>();
            builder.Services.AddSingleton<Services.IServices.IStorageService, Services.Services.StorageService>();
            builder.Services.AddSingleton<Services.IServices.IFileService, Services.Services.StorageService>();
            builder.Services.AddSingleton<Services.IServices.IDownloadService, Services.Services.DownloadService>();
            builder.Services.AddSingleton<ViewModels.DownloadPageViewModel>();
            builder.Services.AddSingleton<View.Download>();

            return builder.Build();
        }
    }
}
