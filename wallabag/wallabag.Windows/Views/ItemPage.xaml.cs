using System;
using wallabag.Common;
using wallabag.DataModel;
using wallabag.ViewModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace wallabag.Views
{
    public sealed partial class ItemPage : basicPage
    {
        private DataTransferManager dataTransferManager;
        public ItemPage()
        {
            this.InitializeComponent();

            // To handle the share events (Share charm), we load the DataTransferManager.
            dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += dataTransferManager_DataRequested;
        }

        protected override void ChangedSize(double width, double height)
        {
            // This time, we only change the appereance of the back button.
            if (width >= 500 || ApplicationView.GetForCurrentView().Orientation == ApplicationViewOrientation.Portrait)
            {
                VisualStateManager.GoToState(this, "Narrow", false);
            }
            if (width >= 1100 && ApplicationView.GetForCurrentView().Orientation == ApplicationViewOrientation.Landscape)
            {
                VisualStateManager.GoToState(this, "Full", false);
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // When navigated to the page, we set the DataContext of this page to a new type called ItemPageViewModel with the parameter.
            if (e.Parameter != null)
                this.DataContext = new ItemPageViewModel() { Item = await wallabagDataSource.GetItemAsync(e.Parameter as string) };

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            dataTransferManager.DataRequested -= dataTransferManager_DataRequested;
            base.OnNavigatedFrom(e);
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
            // Opens links in the Internet Explorer and not in the webView.
            if (args.Uri != null && args.Uri.AbsoluteUri.StartsWith("http"))
            {
                args.Cancel = true;
                await Launcher.LaunchUriAsync(new Uri(args.Uri.AbsoluteUri));
            }
        }

        protected override void SaveState(SaveStateEventArgs e)
        {
            e.PageState.Add("ItemId", ((ItemPageViewModel)this.DataContext).Item.UniqueId);
        }
        protected override async void LoadState(LoadStateEventArgs e)
        {
            if (e.PageState != null)
            {
                if (e.PageState.ContainsKey("ItemId"))
                    this.DataContext = new ItemPageViewModel() { Item = await wallabagDataSource.GetItemAsync(e.PageState["ItemId"] as string) };
            }
        }
    }
}
