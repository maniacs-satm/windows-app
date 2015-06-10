using System;
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
        public MainViewModel ViewModel { get { return (MainViewModel)DataContext; } }
        private ItemViewModel _lastSelectedItem;

        public ContentPage()
        {
            InitializeComponent();
        }

        private void ItemGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var clickedItem = (ItemViewModel)e.ClickedItem;

            _lastSelectedItem = clickedItem;
            ViewModel.CurrentItem = clickedItem;

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

            if (ViewModel.CurrentItemIsNotNull)
            {
                ItemGridView.SelectedItem = ViewModel.CurrentItem;
            }

        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            ItemGridView.SelectedItem = _lastSelectedItem;
        }

        private void AdaptiveStates_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
        }

        private async void AddItemAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            await AddItemContentDialog.ShowAsync();
        }
    }
}
