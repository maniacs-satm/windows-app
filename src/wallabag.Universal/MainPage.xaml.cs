using wallabag.Common;
using wallabag.DataModel;
using wallabag.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace wallabag.Universal
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void ItemsMenuButton_Click(object sender, RoutedEventArgs e)
        {
            HeaderTextBlock.Text = "ITEMS";
            BottomAppBar.Visibility = Visibility.Visible;
        }

        private void TagsMenuButton_Click(object sender, RoutedEventArgs e)
        {
            HeaderTextBlock.Text = "TAGS";
            BottomAppBar.Visibility = Visibility.Visible;
        }

        private void SettingsMenuButton_Click(object sender, RoutedEventArgs e)
        {
            HeaderTextBlock.Text = "SETTINGS";
            BottomAppBar.Visibility = Visibility.Collapsed;
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var dataContext = (MainViewModel)DataContext;

            // If there's a previous selected item, unselect it.
            if (dataContext.CurrentItemIsNotNull)
                dataContext.CurrentItem.IsSelected = false;
            
            dataContext.CurrentItem = (ItemViewModel)e.ClickedItem;
            dataContext.CurrentItem.IsSelected = true;

            //Show the content of the clicked item in the WebView.
            WebView.NavigateToString(dataContext.CurrentItem.ContentWithHeader);
        }

        private void ShowInFullscreenButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Views.SingleItemPage), ((MainViewModel)DataContext).CurrentItem.Model.Id);
        }

        private void SaveCredentialsButton_Click(object sender, RoutedEventArgs e)
        {
            AppSettings.Instance.Username = UsernameTextBox.Text;
            AppSettings.Instance.Password = PasswordTextBox.Password;
            AppSettings.Instance.wallabagUrl = UrlTextBox.Text;
        }
    }
}
