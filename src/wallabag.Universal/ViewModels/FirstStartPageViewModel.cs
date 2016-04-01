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

        public bool CredentialsAreExisting { get { return _dataService.CredentialsAreExisting; } }
        public bool CredentialsWereSynced { get; set; }
        public bool AllowTelemetryData { get; set; } = true;

        public string Username { get; set; }
        public string Password { get; set; }
        public string WallabagUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

        public string StatusText { get; set; }
        public DelegateCommand LoginCommand { get; private set; }
        public DelegateCommand OpenImageSourceCommand { get; private set; }
        public DelegateCommand SetupWallabagCommand { get; private set; }

        public FirstStartPageViewModel(IDataService dataService)
        {
            _dataService = dataService;

            LoginCommand = new DelegateCommand(async () => await Login());
            OpenImageSourceCommand = new DelegateCommand(async () => await Launcher.LaunchUriAsync(new Uri("https://www.flickr.com/photos/oneterry/16711663295/")));
            SetupWallabagCommand = new DelegateCommand(() =>
            {
                NavigationService.Navigate(typeof(Views.ContentPage));
                NavigationService.ClearHistory();

                Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = false;
            });
        }
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (!string.IsNullOrWhiteSpace(AppSettings.wallabagUrl))
            {
                WallabagUrl = AppSettings.wallabagUrl;
                ClientId = AppSettings.ClientId;
                ClientSecret = AppSettings.ClientSecret;

                CredentialsWereSynced = true;
            }

            if (await GetDatabaseFileAsync() != null)
                SetupWallabagCommand.Execute();
            else if (_dataService.CredentialsAreExisting)
                await SetupWallabagAsync();
        }
        public async Task Login()
        {
            StatusText = LocalizedString("FirstStartLoggingInLabel");

            if (WallabagUrl.EndsWith("/"))
                WallabagUrl = WallabagUrl.Remove(WallabagUrl.Length - 1);

            AppSettings.wallabagUrl = WallabagUrl;
            AppSettings.ClientId = ClientId;
            AppSettings.ClientSecret = ClientSecret;

            if (await _dataService.LoginAsync(WallabagUrl, Username, Password))
            {
                StatusText = LocalizedString("FirstStartLoginSuccededLabel");
                Messenger.Default.Send(new NotificationMessage("LoginSucceded"));
                await SetupWallabagAsync();
            }
            else
            {
                AppSettings.wallabagUrl = string.Empty;
                StatusText = LocalizedString("FirstStartLoginFailedLabel");
                Messenger.Default.Send(new NotificationMessage("LoginFailed"));
            }

        }
        public async Task SetupWallabagAsync()
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
                    Messenger.Default.Send(new NotificationMessage("ShowTelemetryPermission"));
                }
                else
                    StatusText = i.ErrorCode.Message;
            };
            var result = await downloadTask;
        }
    }
}
