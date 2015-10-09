using Template10.Mvvm;
using wallabag.Common;

namespace wallabag.ViewModels
{
    public class SettingsPageViewModel : ViewModelBase
    {
        public override string ViewModelIdentifier { get; set; } = "SettingsPageViewModel";

        public string Username
        {
            get { return AppSettings.Username; }
            set { AppSettings.Username = value; }
        }
        public string Password
        {
            get { return AppSettings.Password; }
            set { AppSettings.Password = value; }
        }
        public string wallabagUrl
        {
            get { return AppSettings.wallabagUrl; }
            set { AppSettings.wallabagUrl = value; }
        }
        public string AccessToken
        {
            get { return AppSettings.AccessToken; }
            set { AppSettings.AccessToken = value; }
        }

        public double FontSize
        {
            get { return AppSettings.FontSize; }
            set { AppSettings.FontSize = value; }
        }
        public string FontFamily
        {
            get { return AppSettings.FontFamily; }
            set { AppSettings.FontFamily = value; }
        }
        public string ColorScheme
        {
            get { return AppSettings.ColorScheme; }
            set { AppSettings.ColorScheme = value; }
        }

        public bool NavigateBackAfterReadingAnArticle
        {
            get { return AppSettings.NavigateBackAfterReadingAnArticle; }
            set { AppSettings.NavigateBackAfterReadingAnArticle = value; }
        }
        public bool SyncReadingProgress
        {
            get { return AppSettings.SyncReadingProgress; }
            set { AppSettings.SyncReadingProgress = value; }
        }
        public bool SyncOnStartup
        {
            get { return AppSettings.SyncOnStartup; }
            set { AppSettings.SyncOnStartup = value; }
        }
        public bool UseClassicContextMenuForMouseInput
        {
            get { return AppSettings.UseClassicContextMenuForMouseInput; }
            set { AppSettings.UseClassicContextMenuForMouseInput = value; }
        }
        public bool UseBackgroundTask
        {
            get { return AppSettings.UseBackgroundTask; }
            set
            {
                AppSettings.UseBackgroundTask = value;

                if (value)
                    Services.BackgroundTaskService.RegisterSyncItemsBackgroundTask();
                else
                    Services.BackgroundTaskService.UnregisterSyncItemsBackgroundTask();
            }
        }     
        public uint BackgroundTaskInterval
        {
            get { return AppSettings.BackgroundTaskInterval; }
            set
            {
                AppSettings.BackgroundTaskInterval = value;

                Services.BackgroundTaskService.UnregisterSyncItemsBackgroundTask();
                Services.BackgroundTaskService.RegisterSyncItemsBackgroundTask();
            }
        }
    }
}
