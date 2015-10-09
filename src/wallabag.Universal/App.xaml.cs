using System;
using System.Threading.Tasks;
using Template10.Common;
using wallabag.Common;
using wallabag.Services;
using Windows.ApplicationModel.Activation;
using Windows.UI.Notifications;

namespace wallabag.Universal
{
    sealed partial class App : BootStrapper
    {
        public App() : base()
        {
            InitializeComponent();
        }

        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            if (startKind == StartKind.Launch)
            {
                DataService.LastUserSyncDateTime = DateTime.Now;
                BadgeUpdateManager.CreateBadgeUpdaterForApplication().Clear();
            }

            if (!string.IsNullOrEmpty(AppSettings.Username) &&
                !string.IsNullOrEmpty(AppSettings.wallabagUrl))
            {
                await DataService.InitializeDatabaseAsync();
                NavigationService.Navigate(typeof(Views.ContentPage));
            }
            else
                NavigationService.Navigate(typeof(Views.FirstStartPage));
        }
    }
}
