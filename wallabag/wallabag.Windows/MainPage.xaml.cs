using System;
using wallabag.Common;
using wallabag.DataModel;
using wallabag.ViewModels;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace wallabag
{
    public sealed partial class MainPage : basicPage
    {
        private bool ItemCommandsAreVisible = false;
        private DataTransferManager dataTransferManager;
        public MainPage()
        {
            this.InitializeComponent();
            dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += dataTransferManager_DataRequested;
        }

        void dataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            if ((this.DataContext as MainViewModel).CurrentItem == null)
                request.FailWithDisplayText("No item selected.");
            else
            {
                Item item = (this.DataContext as MainViewModel).CurrentItem.Model as Item;
                request.Data.Properties.Title = item.Title; // The title of the shared information.
                request.Data.SetWebLink(new Uri(item.Url)); // Setting the Web link to the URL of the saved article.
                var htmlFormat = Windows.ApplicationModel.DataTransfer.HtmlFormatHelper.CreateHtmlFormat(item.Content);
                request.Data.SetHtmlFormat(htmlFormat);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            dataTransferManager.DataRequested -= dataTransferManager_DataRequested;
            base.OnNavigatedFrom(e);
        }

        private void unreadItemsMenuButton_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (SelectedHeaderTextBlock != null)
                this.SelectedHeaderTextBlock.Text = Helpers.LocalizedString("unreadItemsMenuButtonText.Text");
        }
        private void favouriteItemsMenuButton_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (SelectedHeaderTextBlock != null)
                this.SelectedHeaderTextBlock.Text = Helpers.LocalizedString("favouriteItemsMenuButtonText.Text");
        }
        private void archivedItemsMenuButton_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (SelectedHeaderTextBlock != null)
                this.SelectedHeaderTextBlock.Text = Helpers.LocalizedString("archivedItemsMenuButtonText.Text");
        }
        private void tagsMenuButton_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (SelectedHeaderTextBlock != null)
                this.SelectedHeaderTextBlock.Text = Helpers.LocalizedString("tagsMenuButtonText.Text");
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            ((MainViewModel)this.DataContext).CurrentItem = (ItemViewModel)e.ClickedItem;
            webView.NavigateToString(((ItemViewModel)e.ClickedItem).ContentWithHeader);
        }

        private void ItemCommandsViewButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (!ItemCommandsAreVisible)
            {
                ItemActionExpandStoryboard.Begin();
                ItemCommandsAreVisible = true;
            }
            else
            {
                ItemActionExpandStoryboardReverse.Begin();
                ItemCommandsAreVisible = false;
            }
        }

        private void FullScreenButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Views.SingleItemPage), (((MainViewModel)this.DataContext).CurrentItem.Model.Id));
        }

    }
}
