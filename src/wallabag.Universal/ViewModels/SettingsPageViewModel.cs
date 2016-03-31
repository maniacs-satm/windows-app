using System.Linq;
using Template10.Mvvm;
using wallabag.Common;
using Windows.Security.Credentials;
using Windows.Storage;

namespace wallabag.ViewModels
{
    public class SettingsPageViewModel : ViewModelBase
    {
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
        public bool OpenTheFilterPaneWithTheSearch
        {
            get { return AppSettings.OpenTheFilterPaneWithTheSearch; }
            set { AppSettings.OpenTheFilterPaneWithTheSearch = value; }
        }

        public DelegateCommand LogoutCommand { get; private set; }
        public DelegateCommand DeleteDatabaseCommand { get; private set; }

        public SettingsPageViewModel()
        {
            LogoutCommand = new DelegateCommand(() =>
            {
                var vault = new PasswordVault();
                vault.Remove(vault.RetrieveAll().First());

                AppSettings.ClientId = string.Empty;
                AppSettings.ClientSecret = string.Empty;

                ApplicationData.Current.RoamingSettings.DeleteContainer(SingleItemPageViewModel.ContainerKey);

                DeleteDatabaseCommand.Execute();
            });
            DeleteDatabaseCommand = new DelegateCommand(() =>
            {
                AppSettings.DeleteDatabaseOnNextStartup = true;
                Windows.UI.Xaml.Application.Current.Exit();
            });
        }
    }
}
