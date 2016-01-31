using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PropertyChanged;
using Template10.Mvvm;
using wallabag.Common;
using Windows.System;
using static wallabag.Common.Helpers;
using Windows.UI.Xaml.Navigation;
using wallabag.Data.Interfaces;

namespace wallabag.ViewModels
{
    [ImplementPropertyChanged]
    public class FirstStartPageViewModel : ViewModelBase
    {
        private IDataService _dataService;

        public string Username { get; set; }
        public string Password { get; set; }
        public string WallabagUrl { get; set; }

        public string StatusText { get; set; }
        public DelegateCommand LoginCommand { get; private set; }
        public DelegateCommand OpenImageSourceCommand { get; private set; }

        public async Task Login()
        {
            StatusText = LocalizedString("FirstStartLoggingInLabel");
            if (WallabagUrl.EndsWith("/"))
                WallabagUrl = WallabagUrl.Remove(WallabagUrl.Length - 1);

            if (Helpers.IsPhone)
                AppSettings.FontSize += 28;

            if (await Services.AuthorizationService.RequestTokenAsync(Username, Password, WallabagUrl))
            {
                StatusText = LocalizedString("FirstStartLoginSuccededLabel");
                AppSettings.wallabagUrl = WallabagUrl;
                await SetupWallabagAndNavigateToContentPage();
            }
            else
                StatusText = LocalizedString("FirstStartLoginFailedLabel");

        }
        public async Task SetupWallabagAndNavigateToContentPage()
        {
            StatusText = LocalizedString("FirstStartCreateDatabaseLabel");
            await _dataService.InitializeDatabaseAsync();

            StatusText = LocalizedString("FirstStartDownloadArticlesFromServerLabel");
            var downloadTask = _dataService.DownloadItemsFromServerAsync(true);
            downloadTask.Progress = (s, p) =>
            {
                StatusText = string.Format(LocalizedString("FirstStartDownloadProgressLabel"), p.CurrentItemIndex, p.TotalNumberOfItems);
            };
            downloadTask.Completed = (i, s) =>
            {
                if (i.ErrorCode == null)
                {
                    StatusText = LocalizedString("FirstStartFinishedDownloadingLabel");
                    NavigationService.Navigate(typeof(Views.ContentPage));
                    NavigationService.ClearHistory();

                    Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = false;
                }
                else
                    StatusText = i.ErrorCode.Message;
            };
            var result = await downloadTask;
        }

        public FirstStartPageViewModel(IDataService dataService)
        {
            _dataService = dataService;

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
