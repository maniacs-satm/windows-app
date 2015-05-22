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
        private ItemViewModel _lastSelectedItem;

        public ContentPage()
        {
            InitializeComponent();
        }

        private void ItemListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var clickedItem = (ItemViewModel)e.ClickedItem;

            _lastSelectedItem = clickedItem;
            (this.DataContext as MainViewModel).CurrentItem = clickedItem;

           
                Frame.Navigate(typeof(SingleItemPage), clickedItem.Model.Id, new DrillInNavigationTransitionInfo());
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter != null && e.Parameter.GetType() == typeof(int))
            {
                var id = (int)e.Parameter;
                _lastSelectedItem = await DataSource.GetItemAsync(id);
            }

            var dc = (MainViewModel)this.DataContext;
            if (dc.CurrentItemIsNotNull)
            {
                ItemListView.SelectedItem = dc.CurrentItem;
            }

        }
        

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            ItemListView.SelectedItem = _lastSelectedItem;
        }

        private void AdaptiveStates_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
        }
    }
}
