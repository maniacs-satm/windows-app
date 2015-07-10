using wallabag.DataModel;
using wallabag.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace wallabag.Views
{
    public sealed partial class ContentPage : Page
    {
        public MainViewModel ViewModel
        {
            get { return (MainViewModel)DataContext; }
            set { DataContext = value; }
        }

        public ContentPage()
        {
            InitializeComponent();
        }

        private void ItemGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var clickedItem = (ItemViewModel)e.ClickedItem;
            ViewModel.CurrentItem = clickedItem;

            Frame.Navigate(typeof(SingleItemPage), clickedItem.Model.Id, new DrillInNavigationTransitionInfo());
        }              

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            ItemGridView.SelectedItem = ViewModel.CurrentItem;
        }
    }
}
