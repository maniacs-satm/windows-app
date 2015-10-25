using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PropertyChanged;
using Template10.Mvvm;
using wallabag.Common;
using Windows.System;
using Windows.UI.Xaml.Navigation;

namespace wallabag.ViewModels
{
    [ImplementPropertyChanged]
    public class FirstStartPageViewModel : ViewModelBase
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string WallabagUrl { get; set; }

        public string StatusText { get; set; }
        public DelegateCommand LoginCommand { get; private set; }
        public DelegateCommand OpenImageSourceCommand { get; private set; }

        public async Task Login()
        {
            StatusText = "Logging in…";
            if (WallabagUrl.EndsWith("/"))
                WallabagUrl = WallabagUrl.Remove(WallabagUrl.Length - 1);

            if (Helpers.IsPhone)
                AppSettings.FontSize += 28;

            if (await Services.AuthorizationService.RequestTokenAsync(Username, Password, WallabagUrl))
            {
                StatusText = "Logged in.";
                AppSettings.wallabagUrl = WallabagUrl;
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
            var downloadTask = Services.DataService.DownloadItemsFromServerAsync(true);
            downloadTask.Progress = (s, p) =>
            {
                StatusText = $"Downloading item {p.CurrentItemIndex} of {p.TotalNumberOfItems}";
            };
            downloadTask.Completed = (i, s) =>
            {
                if (i.ErrorCode == null)
                {
                    StatusText = "Finished downloading. Enjoy :)";
                    NavigationService.Navigate(typeof(Views.ContentPage));
                    NavigationService.ClearHistory();

                    Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = false;
                }
                else
                    StatusText = i.ErrorCode.Message;
            };
            var result = await downloadTask;
        }

        public FirstStartPageViewModel()
        {
            LoginCommand = new DelegateCommand(async () => await Login());
            OpenImageSourceCommand = new DelegateCommand(async () => await Launcher.LaunchUriAsync(new Uri("https://www.flickr.com/photos/oneterry/16711663295/")));
        }

        public override async void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            bool databaseDoesNotExist = await Windows.Storage.ApplicationData.Current.LocalFolder.TryGetItemAsync(Helpers.DATABASE_FILENAME) == null;
            if (!string.IsNullOrEmpty(AppSettings.AccessToken) && databaseDoesNotExist)
                await SetupWallabagAndNavigateToContentPage();
        }
    }
}
