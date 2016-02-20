using Newtonsoft.Json;
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

        public override async Task OnInitializeAsync(IActivatedEventArgs args)
        {
            if (args.Kind == ActivationKind.ShareTarget)
            {
                NavigationServiceFactory(BackButton.Ignore, ExistingContent.Exclude, CreateRootFrame(args));
                Window.Current.Content = NavigationService.Frame;
                var shareOperation = (args as ShareTargetActivatedEventArgs).ShareOperation;
                var shareOperationSerialized = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(shareOperation));
                NavigationService.Navigate(typeof(Views.AddItemPage), shareOperationSerialized);
            }
        }

        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            if (startKind == StartKind.Launch)
            {
                BadgeUpdateManager.CreateBadgeUpdaterForApplication().Clear();

                if (ViewModels.ViewModelLocator.CurrentDataService.CredentialsAreExisting &&
                    await Windows.Storage.ApplicationData.Current.LocalFolder.TryGetItemAsync(Helpers.DATABASE_FILENAME) != null)
                {
                    NavigationService.Navigate(typeof(Views.ContentPage));
                }
                else
                    NavigationService.Navigate(typeof(Views.FirstStartPage));
            }
        }
    }
}
