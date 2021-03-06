﻿using System;
using System.Threading.Tasks;
using Template10.Common;
using wallabag.Common;
using Windows.ApplicationModel.Activation;
using Windows.UI.Notifications;
using Windows.UI.Xaml;

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
            if (AppSettings.AllowTelemetryData)
                Microsoft.HockeyApp.HockeyClient.Current.Configure("842955f8fd3b4191972db776265d81c4");

            if (args.Kind == ActivationKind.ShareTarget)
            {
                NavigationServiceFactory(BackButton.Ignore, ExistingContent.Exclude, CreateRootFrame(args));
                Window.Current.Content = NavigationService.Frame;
                var shareOperation = args as ShareTargetActivatedEventArgs;
                SessionState.Add("ShareOperation", shareOperation.ShareOperation);
                NavigationService.Navigate(typeof(Views.AddItemPage));
            }
            return Task.CompletedTask;
        }

        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            if (AppSettings.DeleteDatabaseOnNextStartup)
            {
                var databaseFile = await Windows.Storage.StorageFile.GetFileFromPathAsync(Helpers.DATABASE_PATH);
                await databaseFile.DeleteAsync();
                AppSettings.DeleteDatabaseOnNextStartup = false;
            }

            if (startKind == StartKind.Launch)
            {
                BadgeUpdateManager.CreateBadgeUpdaterForApplication().Clear();
                AppSettings.LastOpeningDateTime = DateTime.Now;

                if (ViewModels.ViewModelLocator.CurrentDataService.CredentialsAreExisting && await Helpers.GetDatabaseFileAsync() != null)
                    NavigationService.Navigate(typeof(Views.ContentPage));
                else
                    NavigationService.Navigate(typeof(Views.FirstStartPage));
            }
        }
    }
}
