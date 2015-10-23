using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PropertyChanged;
using Template10.Mvvm;
using wallabag.Common;
using Windows.UI.Xaml.Navigation;

namespace wallabag.ViewModels
{
    [ImplementPropertyChanged]
    class FirstStartPageViewModel : ViewModelBase
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string wallabagUrl { get; set; }

        public string StatusText { get; set; }

        public DelegateCommand LoginCommand { get; private set; }
        public async Task Login()
        {
            StatusText = "Logging in…";
            if (wallabagUrl.EndsWith("/"))
                wallabagUrl = wallabagUrl.Remove(wallabagUrl.Length - 1);

            if (Helpers.IsPhone)
                AppSettings.FontSize += 28;

            if (await Services.AuthorizationService.RequestTokenAsync(Username, Password, wallabagUrl))
            {
                StatusText = "Logged in.";
                AppSettings.wallabagUrl = wallabagUrl;
                await SetupWallabagAndNavigateToContentPage();
            }
            else
                StatusText = "Login failed.";

        }

        public async Task SetupWallabagAndNavigateToContentPage()
        {
            StatusText = "Create database…";
            await Services.DataService.InitializeDatabaseAsync();

            StatusText = "Downloading articles from server…";
            var downloadTask = Services.DataService.DownloadItemsFromServerAsync();
            downloadTask.Progress = (s, p) =>
            {
                StatusText = $"Downloading item {p.CurrentItemIndex} of {p.TotalNumberOfItems}";
                System.Diagnostics.Debug.WriteLine(StatusText);
            };
            downloadTask.Completed = (i, s) =>
            {
                StatusText = "Finished downloading.";
            };
            var result = await downloadTask;
            NavigationService.Navigate(typeof(Views.ContentPage));
            NavigationService.ClearHistory();

            Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = false;
        }

        public FirstStartPageViewModel()
        {
            LoginCommand = new DelegateCommand(async () => await Login());
        }

        public override async void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            bool databaseDoesNotExist = await Windows.Storage.ApplicationData.Current.LocalFolder.TryGetItemAsync(Helpers.DATABASE_FILENAME) == null;
            if (!string.IsNullOrEmpty(AppSettings.AccessToken) && databaseDoesNotExist)
                await SetupWallabagAndNavigateToContentPage();
        }
    }
}
