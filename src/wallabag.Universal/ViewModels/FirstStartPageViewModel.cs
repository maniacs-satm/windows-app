using GalaSoft.MvvmLight.Messaging;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Template10.Mvvm;
using wallabag.Common;
using wallabag.Data.Interfaces;
using Windows.System;
using Windows.UI.Xaml.Navigation;
using static wallabag.Common.Helpers;

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

        public FirstStartPageViewModel(IDataService dataService)
        {
            _dataService = dataService;

            LoginCommand = new DelegateCommand(async () => await Login());
            OpenImageSourceCommand = new DelegateCommand(async () => await Launcher.LaunchUriAsync(new Uri("https://www.flickr.com/photos/oneterry/16711663295/")));
        }
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (_dataService.CredentialsAreExisting)
                await SetupWallabagAndNavigateToContentPage();
        }
        public async Task Login()
        {
            StatusText = LocalizedString("FirstStartLoggingInLabel");
            if (WallabagUrl.EndsWith("/"))
                WallabagUrl = WallabagUrl.Remove(WallabagUrl.Length - 1);

            if (IsPhone)
                AppSettings.FontSize += 28;

            if (await _dataService.LoginAsync(WallabagUrl, Username, Password))
            {
                StatusText = LocalizedString("FirstStartLoginSuccededLabel");
                AppSettings.wallabagUrl = WallabagUrl;
                await SetupWallabagAndNavigateToContentPage();
            }
            else
            {
                StatusText = LocalizedString("FirstStartLoginFailedLabel");
                Messenger.Default.Send(new NotificationMessage("LoginFailed"));
            }

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
    }
}
