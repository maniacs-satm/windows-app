using wallabag.Common;
using wallabag.DataModel;
using Windows.UI.ApplicationSettings;
using Windows.UI.ViewManagement;
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

    }
}
