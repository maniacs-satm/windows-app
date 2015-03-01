using System;
using wallabag.Common;
using wallabag.ViewModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using wallabag.DataModel;

namespace wallabag.Views
{
    public sealed partial class ItemPage : basicPage
    {
        public ItemPage()
        {
            this.InitializeComponent();

            // To handle the share events (Share charm), we load the DataTransferManager.
            var dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += dataTransferManager_DataRequested;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // When navigated to the page, we set the DataContext of this page to a new type called ItemPageViewModel with the parameter.
            if (e.Parameter != null)
                this.DataContext = new ItemPageViewModel() { Item = await wallabagDataSource.GetItemAsync(e.Parameter as string) };

            base.OnNavigatedTo(e);

            // Hide the status bar (clock, battery, connection informations, etc.)
            var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
            await statusBar.HideAsync();
        }

        void dataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            Item item = (this.DataContext as ItemPageViewModel).Item as Item;
            request.Data.Properties.Title = item.Title; // The title of the shared information.
            request.Data.SetWebLink(item.Url); // Setting the Web link to the URL of the saved article.
            request.Data.SetHtmlFormat(item.Content); // If the target app supports it, it is also possible to send the content.
        }

        private async void webView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            this.LoadingIndicator.Visibility = Windows.UI.Xaml.Visibility.Visible;
            // Opens links in the Internet Explorer and not in the webView.
            if (args.Uri != null && args.Uri.AbsoluteUri.StartsWith("http"))
            {
                args.Cancel = true;
                this.LoadingIndicator.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                await Launcher.LaunchUriAsync(new Uri(args.Uri.AbsoluteUri));
            }
        }

        private void webView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            this.LoadingIndicator.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }
    }
}
