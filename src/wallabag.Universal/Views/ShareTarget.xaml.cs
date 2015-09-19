using System;
using System.Collections.Generic;
using wallabag.Common;
using wallabag.Common.Mvvm;
using wallabag.Models;
using wallabag.Services;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace wallabag.Views
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet werden kann oder auf die innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class ShareTarget : Page
    {
        private ShareOperation shareOperation;

        public string Url { get; set; }
        public ICollection<Tag> Tags { get; set; }

        public Command AddItemCommand { get; private set; }
        public Command CancelCommand { get; private set; }

        public ShareTarget()
        {
            InitializeComponent();
            AddItemCommand = new Command(async () =>
            {
                addItemAppBarButton.IsEnabled = false;
                urlTextBox.IsEnabled = false;
                tagControl.IsEnabled = false;
                savingIndicator.Visibility = Windows.UI.Xaml.Visibility.Visible;

                await DataService.AddItemAsync(Url, Tags.ToCommaSeparatedString());
                shareOperation.ReportCompleted();
            });
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            shareOperation = (ShareOperation)e.Parameter;
            Url = (await shareOperation.Data.GetWebLinkAsync()).ToString();

            if (Helpers.IsPhone)
                Windows.UI.ViewManagement.StatusBar.GetForCurrentView().BackgroundColor = (Color)Windows.UI.Xaml.Application.Current.Resources["SystemAccentColor"];
        }
    }
}
