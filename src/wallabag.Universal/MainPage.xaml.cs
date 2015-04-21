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
            HeaderTextBlock.Text = "Items";
            itemGrid.Visibility = Visibility.Visible; // ensure that the itemGrid is visible, even if the FavoriteMenuButton is clicked
            BottomAppBar.Visibility = Visibility.Visible;
        }

        private void TagsMenuButton_Click(object sender, RoutedEventArgs e)
        {
            HeaderTextBlock.Text = "Tags";
            BottomAppBar.Visibility = Visibility.Visible;
        }

        private void SettingsMenuButton_Click(object sender, RoutedEventArgs e)
        {
            HeaderTextBlock.Text = "Settings";
            BottomAppBar.Visibility = Visibility.Collapsed;
        }
    }
}
