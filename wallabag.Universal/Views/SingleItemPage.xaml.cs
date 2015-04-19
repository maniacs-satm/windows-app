using System;
using wallabag.DataModel;
using wallabag.ViewModels;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace wallabag.Views
{
    public sealed partial class SingleItemPage : Common.basicPage
    {
        private DataTransferManager dataTransferManager;
        public SingleItemPage()
        {
            this.InitializeComponent();
            dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += dataTransferManager_DataRequested;
        }

        void dataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            Item item = (this.DataContext as SingleItemPageViewModel).CurrentItem.Model as Item;
            request.Data.Properties.Title = item.Title; // The title of the shared information.
            request.Data.SetWebLink(new Uri(item.Url)); // Setting the Web link to the URL of the saved article.
            var htmlFormat = Windows.ApplicationModel.DataTransfer.HtmlFormatHelper.CreateHtmlFormat(item.Content);
            request.Data.SetHtmlFormat(htmlFormat);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
                DataContext = new ViewModels.SingleItemPageViewModel() { CurrentItem = await wallabag.DataModel.DataSource.GetItemAsync((int)e.Parameter) };
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            dataTransferManager.DataRequested -= dataTransferManager_DataRequested;
            base.OnNavigatedFrom(e);
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
        }
    }
}
