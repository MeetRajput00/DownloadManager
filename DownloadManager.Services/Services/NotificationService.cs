using CommunityToolkit.Maui.Alerts;

namespace DownloadManager.Services.Services
{
    public static class NotificationService
    {
        public static void ShowFileSavedError()
        {
            Toast.Make($"The file was not saved successfully.").Show();
        }

        public static void ShowFileSavedSuccessfully()
        {
            Toast.Make($"The file was saved successfully").Show();
        }

        public static void ShowInvalidUrlPopUp()
        {
            Toast.Make("Invalid Url!").Show();
        }
    }
}
