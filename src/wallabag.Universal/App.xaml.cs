using System;
using System.Threading.Tasks;
using Template10.Common;
using wallabag.Common;
using wallabag.Services;
using Windows.ApplicationModel.Activation;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace wallabag.Universal
{
    sealed partial class App : BootStrapper
    {
        public App() : base()
        {
            InitializeComponent();
        }

        public override Task OnInitializeAsync(IActivatedEventArgs args)
        {
            if (args.Kind == ActivationKind.ShareTarget)
            {
                var frame = new Frame();
                Window.Current.Content = frame;
                frame.Navigate(typeof(Views.AddItemPage), (args as ShareTargetActivatedEventArgs).ShareOperation);
                return Task.FromResult<object>(null);
            }
            return base.OnInitializeAsync(args);
        }

        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            if (startKind == StartKind.Launch)
            {
                DataService.LastUserSyncDateTime = DateTime.Now;
                BadgeUpdateManager.CreateBadgeUpdaterForApplication().Clear();
            }

            if (!string.IsNullOrEmpty(AppSettings.AccessToken))
            {
                await DataService.InitializeDatabaseAsync();
                NavigationService.Navigate(typeof(Views.ContentPage));
            }
            else
                NavigationService.Navigate(typeof(Views.FirstStartPage));
        }
    }
}
