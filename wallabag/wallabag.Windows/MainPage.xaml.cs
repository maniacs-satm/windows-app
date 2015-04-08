using wallabag.Common;
using wallabag.DataModel;
using wallabag.ViewModels;
using Windows.UI.Xaml.Controls;

namespace wallabag
{
    public sealed partial class MainPage : basicPage
    {
        public MainPage()
        {
            this.InitializeComponent();
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
            webView.NavigateToString(((ItemViewModel)e.ClickedItem).ContentWithTitle);
        }

    }
}
