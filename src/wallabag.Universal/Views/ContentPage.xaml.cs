using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Template10.Common;
using Template10.Services.NavigationService;
using wallabag.Common;
using wallabag.Models;
using wallabag.Services;
using wallabag.ViewModels;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace wallabag.Views
{
    public sealed partial class ContentPage : Page
    {
        public MainViewModel ViewModel { get { return (MainViewModel)DataContext; } }
        private GridView _ItemGridView;
        public bool IsMultipleSelectionEnabled { get; set; } = false;
        private bool IsSearchVisible { get; set; } = false;

        #region Context menu
        private bool _IsShiftPressed = false;
        private bool _IsPointerPressed = false;
        private ItemViewModel _LastFocusedItemViewModel;
        private FrameworkElement _LastFocusedItem;

        protected override void OnKeyDown(KeyRoutedEventArgs e)
        {
            // Handle Shift+F10
            // Handle MenuKey

            if (e.Key == VirtualKey.Shift)
                _IsShiftPressed = true;

            // Shift+F10 or the 'Menu' key next to Right Ctrl on most keyboards
            else if (_IsShiftPressed && e.Key == VirtualKey.F10
                    || e.Key == VirtualKey.Application)
            {
                var FocusedUIElement = FocusManager.GetFocusedElement() as UIElement;
                if (FocusedUIElement is ContentControl)
                    _LastFocusedItemViewModel = ((ContentControl)FocusedUIElement).Content as ItemViewModel;

                ShowContextMenu(_LastFocusedItemViewModel, FocusedUIElement, new Point(0, 0));
                e.Handled = true;
            }

            base.OnKeyDown(e);
        }
        protected override void OnKeyUp(KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Shift)
                _IsShiftPressed = false;

            base.OnKeyUp(e);
        }
        protected override void OnHolding(HoldingRoutedEventArgs e)
        {
            ShowContextMenu(e.OriginalSource as FrameworkElement);
            _LastFocusedItemViewModel = (e.OriginalSource as FrameworkElement).DataContext as ItemViewModel;

            e.Handled = true;

            base.OnHolding(e);
        }
        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            _IsPointerPressed = true;

            base.OnPointerPressed(e);
        }
        protected override void OnRightTapped(RightTappedRoutedEventArgs e)
        {
            if (_IsPointerPressed)
            {
                _LastFocusedItemViewModel = (e.OriginalSource as FrameworkElement).DataContext as ItemViewModel;

                if (AppSettings.UseClassicContextMenuForMouseInput)
                    ShowContextMenu(_LastFocusedItemViewModel, null, e.GetPosition(null));
                else
                    ShowContextMenu(e.OriginalSource as FrameworkElement);

                e.Handled = true;
            }

            base.OnRightTapped(e);
        }
        private void ShowContextMenu(ItemViewModel data, UIElement target, Point offset)
        {
            if (data != null && !IsMultipleSelectionEnabled)
            {
                var MyFlyout = Resources["ItemContextMenu"] as MenuFlyout;
                MyFlyout.ShowAt(target, offset);
            }
        }
        private void ShowContextMenu(FrameworkElement element)
        {
            if (IsMultipleSelectionEnabled)
                return;

            if (_LastFocusedItem != null)
                ((_LastFocusedItem as Grid).Resources["HideContextMenu"] as Storyboard).Begin();

            _LastFocusedItem = element;
            if (element.GetType() == typeof(Grid))
            {
                var grid = element as Grid;
                if (grid.Name == "ContextMenuGrid")
                    (grid.Resources["ShowContextMenu"] as Storyboard).Begin();

            }
        }

        private void ScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            if (_LastFocusedItem != null)
            {
                ((_LastFocusedItem as Grid).Resources["HideContextMenu"] as Storyboard).Begin();
                _LastFocusedItem = null;
            }
        }

        private async void ContextMenuMarkAsRead_Click(object sender, RoutedEventArgs e)
            => await _LastFocusedItemViewModel.SwitchReadValueAsync();
        private async void ContextMenuMarkAsFavorite_Click(object sender, RoutedEventArgs e)
            => await _LastFocusedItemViewModel.SwitchFavoriteValueAsync();
        private void ContextMenuShareItem_Click(object sender, RoutedEventArgs e)
            => _LastFocusedItemViewModel.ShareCommand.Execute(null);
        private async void ContextMenuOpenInBrowser_Click(object sender, RoutedEventArgs e)
            => await Launcher.LaunchUriAsync(new Uri(_LastFocusedItemViewModel.Model.Url));
        private async void ContextMenuDeleteItem_Click(object sender, RoutedEventArgs e)
            => await _LastFocusedItemViewModel.DeleteItemAsync();
        #endregion

        public ObservableCollection<KeyValuePair<int, string>> PossibleSearchBoxResults { get; set; } = new ObservableCollection<KeyValuePair<int, string>>();
        public ObservableCollection<KeyValuePair<int, string>> SearchBoxSuggestions { get; set; } = new ObservableCollection<KeyValuePair<int, string>>();

        public ICollection<Tag> MultipleSelectionTags { get; set; }

        public ContentPage()
        {
            InitializeComponent();
            (Resources["HideAddItemBorder"] as Storyboard).Completed += HideAddItemBorder_Completed;
            MultipleSelectionTags = new ObservableCollection<Tag>();
        }

        private async void HideAddItemBorder_Completed(object sender, object e) => await ViewModel.LoadItemsAsync();
        private void ItemGridView_Loaded(object sender, RoutedEventArgs e) => _ItemGridView = sender as GridView;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            List<Item> allItems = await DataService.GetItemsAsync(new FilterProperties() { ItemType = FilterProperties.FilterPropertiesItemType.All });
            foreach (var item in allItems)
                PossibleSearchBoxResults.Add(new KeyValuePair<int, string>(item.Id, item.Title));
        }

        private void ItemGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var clickedItem = (ItemViewModel)e.ClickedItem;
            (Application.Current as BootStrapper).NavigationService.Navigate(typeof(SingleItemPage), clickedItem.Model.Id.ToString());
        }

        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var possibleResults = new ObservableCollection<KeyValuePair<int, string>>(PossibleSearchBoxResults.Where(t => t.Value.ToLower().Contains(sender.Text.ToLower())));

            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                SearchBoxSuggestions.Clear();
                foreach (var item in possibleResults)
                    SearchBoxSuggestions.Add(item);
            }
        }

        private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                var id = ((KeyValuePair<int, string>)args.ChosenSuggestion).Key;
                (Application.Current as BootStrapper).NavigationService.Navigate(typeof(SingleItemPage), id.ToString());
            }
            // TODO: Implement a search page in case the user didn't chose a suggestion.
        }

        private void AddItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (Window.Current.Bounds.Width <= 500 || Helpers.IsPhone)
                (Application.Current as BootStrapper).NavigationService.Navigate(typeof(AddItemPage));
            else
                (Resources["ShowAddItemBorder"] as Storyboard).Begin();
        }

        #region Multiple selection

        private void multipleSelectToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsMultipleSelectionEnabled)
            {
                IsMultipleSelectionEnabled = false;
                _ItemGridView.SelectionMode = ListViewSelectionMode.None;
                _ItemGridView.IsItemClickEnabled = true;
                MultipleSelectionCommandBar.Visibility = Visibility.Collapsed;
                PrimaryCommandBar.Visibility = Visibility.Visible;
            }
            else
            {
                IsMultipleSelectionEnabled = true;
                _ItemGridView.SelectionMode = ListViewSelectionMode.Multiple;
                _ItemGridView.IsItemClickEnabled = false;
                MultipleSelectionCommandBar.Visibility = Visibility.Visible;
                PrimaryCommandBar.Visibility = Visibility.Collapsed;
            }
        }

        private async void MarkItemsAsReadMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (ItemViewModel item in _ItemGridView.SelectedItems)
            {
                item.Model.IsRead = true;
                await ItemViewModel.UpdateSpecificProperty(item.Model.Id, "archive", true);
            }
            multipleSelectToggleButton_Click(sender, e);
        }
        private async void UnmarkItemsAsReadMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (ItemViewModel item in _ItemGridView.SelectedItems)
            {
                item.Model.IsRead = false;
                await ItemViewModel.UpdateSpecificProperty(item.Model.Id, "archive", false);
            }
            multipleSelectToggleButton_Click(sender, e);
        }
        private async void MarkItemsAsFavoriteMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (ItemViewModel item in _ItemGridView.SelectedItems)
            {
                item.Model.IsStarred = true;
                await ItemViewModel.UpdateSpecificProperty(item.Model.Id, "star", true);
            }
            multipleSelectToggleButton_Click(sender, e);
        }
        private async void UnmarkItemsAsFavoriteMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (ItemViewModel item in _ItemGridView.SelectedItems)
            {
                item.Model.IsStarred = true;
                await ItemViewModel.UpdateSpecificProperty(item.Model.Id, "star", false);
            }
            multipleSelectToggleButton_Click(sender, e);
        }
        private void ManageTagsMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            (Resources["ShowEditTagsBorder"] as Storyboard).Begin();
        }
        private async void DeleteMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (ItemViewModel item in _ItemGridView.SelectedItems)
            {
                item.Model.IsDeleted = true;
                await item.DeleteItemAsync();
            }
            multipleSelectToggleButton_Click(sender, e);
        }

        private void CancelTagsAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            MultipleSelectionTags.Clear();
            (Resources["HideEditTagsBorder"] as Storyboard).Begin();
        }
        private async void SaveTagsAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            (Resources["HideEditTagsBorder"] as Storyboard).Begin();

            foreach (ItemViewModel item in _ItemGridView.SelectedItems)
                await ItemViewModel.AddTagsAsync(item.Model.Id, MultipleSelectionTags.ToCommaSeparatedString());

            MultipleSelectionTags.Clear();
            multipleSelectToggleButton_Click(sender, e);
        }

        #endregion

        private async void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch ((sender as Pivot).SelectedIndex)
            {
                case 0:
                    ViewModel.LastUsedFilterProperties.ItemType = FilterProperties.FilterPropertiesItemType.Unread;
                    await ViewModel.LoadItemsAsync();
                    break;
                case 1:
                    ViewModel.LastUsedFilterProperties.ItemType = FilterProperties.FilterPropertiesItemType.Favorites;
                    await ViewModel.LoadItemsAsync();
                    break;
                case 2:
                    ViewModel.LastUsedFilterProperties.ItemType = FilterProperties.FilterPropertiesItemType.Archived;
                    await ViewModel.LoadItemsAsync();
                    break;
            }
        }

        private void ItemGridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var ItemGridView = sender as GridView;
            if (e.NewSize.Width < 720)
                ItemGridView.ItemsPanel = ListViewTemplate;
            else
                ItemGridView.ItemsPanel = GridViewTemplate;
        }

        private void HideAddItemBorder_Click(object sender, RoutedEventArgs e) =>
            (Resources["HideAddItemBorder"] as Storyboard).Begin();

        #region Filter

        private async void sortOrderRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == sortAscendingRadioButton)
                ViewModel.LastUsedFilterProperties.SortOrder = FilterProperties.FilterPropertiesSortOrder.Ascending;
            else
                ViewModel.LastUsedFilterProperties.SortOrder = FilterProperties.FilterPropertiesSortOrder.Descending;
            await ViewModel.FilterItemsAsync();
        }

        private async void filterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await ViewModel.FilterItemsAsync();
        }

        private async void filterCalendarDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            await ViewModel.FilterItemsAsync();
        }

        private void resetFilterButton_Click(object sender, RoutedEventArgs e)
        {
            sortDescendingRadioButton.IsChecked = true;
        }

        #endregion

        private void searchToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsSearchVisible)
            {
                ShowSearchGrid.Begin();
                IsSearchVisible = true;
            }
            else
            {
                HideSearchGrid.Begin();
                IsSearchVisible = false;
            }
        }

        private void SplitView_PaneClosing(SplitView sender, SplitViewPaneClosingEventArgs args)
        {
            args.Cancel = true;
            HideFilterPanel(sender);
        }
        
        private void HideFilterPanel(SplitView sender)
        {
            if (filterToggleButton.IsChecked == true)
            {
                HideSearchGrid.Begin();
                sender.IsPaneOpen = false;
                IsSearchVisible = false;
            }
        }
    }
}
