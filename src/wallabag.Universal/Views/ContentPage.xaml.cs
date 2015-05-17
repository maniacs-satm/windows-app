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
        private ItemViewModel _lastSelectedItem;

        public ContentPage()
        {
            InitializeComponent();
        }

        private void ItemListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var clickedItem = (ItemViewModel)e.ClickedItem;

            //_lastSelectedItem = clickedItem;
            (this.DataContext as MainViewModel).CurrentItem = clickedItem;

            if (AdaptiveStates.CurrentState == NarrowState)
                Frame.Navigate(typeof(SingleItemPage), clickedItem.Model.Id, new DrillInNavigationTransitionInfo());
            else
                WebView.NavigateToString(clickedItem.ContentWithHeader);
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
                WebView.NavigateToString(dc.CurrentItem.ContentWithHeader);
                ItemListView.SelectedItem = dc.CurrentItem;
            }

            UpdateForVisualState(AdaptiveStates.CurrentState);
        }

        private void UpdateForVisualState(VisualState newState, VisualState oldState = null)
        {
            var isNarrow = newState == NarrowState;

            if (isNarrow && oldState == DefaultState && _lastSelectedItem != null)
            {
                // Resize down to the detail item. Don't play a transition.
                Frame.Navigate(typeof(SingleItemPage), _lastSelectedItem.Model.Id, new SuppressNavigationTransitionInfo());
            }

            EntranceNavigationTransitionInfo.SetIsTargetElement(ItemListView, isNarrow);
            if (WebView != null)
            {
                EntranceNavigationTransitionInfo.SetIsTargetElement(WebView, !isNarrow);
            }
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            ItemListView.SelectedItem = _lastSelectedItem;
        }

        private void AdaptiveStates_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            UpdateForVisualState(e.NewState, e.OldState);
        }
    }
}
