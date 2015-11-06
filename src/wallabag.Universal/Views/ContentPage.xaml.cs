using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Template10.Common;
using wallabag.Common;
using wallabag.Models;
using wallabag.Services;
using wallabag.ViewModels;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
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

        public ObservableCollection<SearchResult> Items { get; set; } = new ObservableCollection<SearchResult>();
        public ObservableCollection<SearchResult> ItemSearchSuggestions { get; set; } = new ObservableCollection<SearchResult>();

        public ObservableCollection<string> DomainNames { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> DomainNameSuggestions { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<Tag> Tags { get; set; } = new ObservableCollection<Tag>();
        public ObservableCollection<Tag> TagSuggestions { get; set; } = new ObservableCollection<Tag>();

        #region Context menu
        private bool _IsShiftPressed = false;
        private bool _IsPointerPressed = false;
        private ItemViewModel _LastFocusedItemViewModel;

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

                ShowContextMenu(FocusedUIElement, new Point(0, 0));
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
            if (e.HoldingState == Windows.UI.Input.HoldingState.Started)
            {
                _LastFocusedItemViewModel = (e.OriginalSource as FrameworkElement).DataContext as ItemViewModel;
                ShowContextMenu(null, e.GetPosition(null));
                e.Handled = true;

                _IsPointerPressed = false;

                var itemsToCancel = VisualTreeHelper.FindElementsInHostCoordinates(e.GetPosition(null), _ItemGridView);
                foreach (var item in itemsToCancel)
                    item.CancelDirectManipulations();
            }

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
                ShowContextMenu(null, e.GetPosition(null));

                e.Handled = true;
            }

            base.OnRightTapped(e);
        }
        private void ShowContextMenu(UIElement target, Point offset)
        {
            if (_LastFocusedItemViewModel != null && !IsMultipleSelectionEnabled)
            {
                var MyFlyout = Resources["ItemContextMenu"] as MenuFlyout;
                MyFlyout.ShowAt(target, offset);
            }
        }

        private async void ContextMenuMarkAsRead_Click(object sender, RoutedEventArgs e)
        {
            await _LastFocusedItemViewModel.SwitchReadValueAsync();
            await ViewModel.RefreshItemsAsync();
        }
        private async void ContextMenuMarkAsFavorite_Click(object sender, RoutedEventArgs e)
        {
            await _LastFocusedItemViewModel.SwitchFavoriteValueAsync();
            await ViewModel.RefreshItemsAsync();
        }
        private void ContextMenuShareItem_Click(object sender, RoutedEventArgs e)
            => _LastFocusedItemViewModel.ShareCommand.Execute(null);
        private async void ContextMenuOpenInBrowser_Click(object sender, RoutedEventArgs e)
            => await Launcher.LaunchUriAsync(new Uri(_LastFocusedItemViewModel.Model.Url));
        private async void ContextMenuDeleteItem_Click(object sender, RoutedEventArgs e)
        {
            await _LastFocusedItemViewModel.DeleteAsync();
            await ViewModel.RefreshItemsAsync();
        }
        #endregion

        public ICollection<Tag> MultipleSelectionTags { get; set; }

        public ContentPage()
        {
            InitializeComponent();
            HideAddItemBorder.Completed += HideAddItemBorder_Completed;
            HideDragDropGridStoryboard.Completed += HideAddItemBorder_Completed;
            MultipleSelectionTags = new ObservableCollection<Tag>();
        }

        private async void HideAddItemBorder_Completed(object sender, object e) => await ViewModel.RefreshItemsAsync();
        private void ItemGridView_Loaded(object sender, RoutedEventArgs e) => _ItemGridView = sender as GridView;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            List<Item> allItems = await DataService.GetItemsAsync(new FilterProperties() { ItemType = FilterProperties.FilterPropertiesItemType.All });
            foreach (var item in allItems)
            {
                Items.Add(new SearchResult(item.Id, item.Title));

                string domainName = item.DomainName.Replace("www.", string.Empty);
                if (!DomainNames.Contains(domainName))
                    DomainNames.Add(domainName);

                foreach (Tag tag in item.Tags)
                    if (Tags.Where(t => t.Label == tag.Label).Count() == 0)
                        Tags.Add(tag);
            }
            if (AppSettings.ShowTheFilterPaneInline && !Helpers.IsPhone)
                splitView.DisplayMode = SplitViewDisplayMode.Inline;
            else
                splitView.DisplayMode = SplitViewDisplayMode.Overlay;
        }

        private void ItemGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem != null)
            {
                var clickedItem = (ItemViewModel)e.ClickedItem;
                BootStrapper.Current.NavigationService.Navigate(typeof(SingleItemPage), clickedItem.Model.Id);
            }
        }

        private void AddItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (Window.Current.Bounds.Width <= 500 || Helpers.IsPhone)
                (Application.Current as BootStrapper).NavigationService.Navigate(typeof(AddItemPage));
            else
                (Resources["ShowAddItemBorder"] as Storyboard).Begin();
        }

        #region Multiple selection

        private async void multipleSelectToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsMultipleSelectionEnabled)
            {
                SQLite.SQLiteAsyncConnection conn = new SQLite.SQLiteAsyncConnection(Helpers.DATABASE_PATH);

                foreach (ItemViewModel item in _ItemGridView.SelectedItems)
                    await conn.UpdateAsync(item.Model);

                await ViewModel.RefreshItemsAsync();

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
                await item.DeleteAsync();
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
                await item.AddTagsAsync(MultipleSelectionTags as IList<Tag>);

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
                    await ViewModel.RefreshItemsAsync();
                    break;
                case 1:
                    ViewModel.LastUsedFilterProperties.ItemType = FilterProperties.FilterPropertiesItemType.Favorites;
                    await ViewModel.RefreshItemsAsync();
                    break;
                case 2:
                    ViewModel.LastUsedFilterProperties.ItemType = FilterProperties.FilterPropertiesItemType.Archived;
                    await ViewModel.RefreshItemsAsync();
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

        #region Search & Filter
        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var possibleResults = new ObservableCollection<SearchResult>(Items.Where(t => t.Value.ToLower().Contains(sender.Text.ToLower())));

            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                ItemSearchSuggestions.Clear();
                foreach (var item in possibleResults)
                    ItemSearchSuggestions.Add(item);
            }
        }
        private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                var id = ((SearchResult)args.ChosenSuggestion).Id;
                (Application.Current as BootStrapper).NavigationService.Navigate(typeof(SingleItemPage), id.ToString());
            }
        }

        private async void domainNameAutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (string.IsNullOrEmpty(sender.Text))
            {
                ViewModel.LastUsedFilterProperties.DomainName = string.Empty;
                await ViewModel.RefreshItemsAsync();
                return;
            }

            var possibleResults = new ObservableCollection<string>(DomainNames.Where(t => t.ToLower().StartsWith(sender.Text.ToLower())));

            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                DomainNameSuggestions.Clear();
                foreach (var item in possibleResults)
                    DomainNameSuggestions.Add(item);
            }
        }
        private async void domainNameAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                sender.Text = args.ChosenSuggestion.ToString();
                ViewModel.LastUsedFilterProperties.DomainName = sender.Text;
                await ViewModel.RefreshItemsAsync();
            }
        }

        private async void tagAutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (string.IsNullOrEmpty(sender.Text))
            {
                ViewModel.LastUsedFilterProperties.FilterTag = null;
                await ViewModel.RefreshItemsAsync();
                return;
            }

            var possibleResults = new ObservableCollection<Tag>(Tags.Where(t => t.Label.ToLower().StartsWith(sender.Text.ToLower())));

            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                TagSuggestions.Clear();
                foreach (var item in possibleResults)
                    TagSuggestions.Add(item);
            }
        }
        private async void tagAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                sender.Text = args.ChosenSuggestion.ToString();
                ViewModel.LastUsedFilterProperties.FilterTag = args.ChosenSuggestion as Tag;
                await ViewModel.RefreshItemsAsync();
            }
        }

        private async void sortOrderRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == sortAscendingRadioButton)
                ViewModel.LastUsedFilterProperties.SortOrder = FilterProperties.FilterPropertiesSortOrder.Ascending;
            else
                ViewModel.LastUsedFilterProperties.SortOrder = FilterProperties.FilterPropertiesSortOrder.Descending;
            await ViewModel.RefreshItemsAsync(false, true);
        }
        private async void filterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await ViewModel.RefreshItemsAsync();
        }
        private async void filterCalendarDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            await ViewModel.RefreshItemsAsync();
        }

        private void resetFilterButton_Click(object sender, RoutedEventArgs e)
        {
            sortDescendingRadioButton.IsChecked = true;
            shortEstimatedReadingTimeRadioButton.IsChecked = null;
            mediumEstimatedReadingTimeRadioButton.IsChecked = null;
            longEstimatedReadingTimeRadioButton.IsChecked = null;
        }
        private void searchToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsSearchVisible)
            {
                ShowSearchGrid.Begin();
                IsSearchVisible = true;
                splitView.IsPaneOpen = AppSettings.OpenTheFilterPaneWithTheSearch;
            }
            else
            {
                HideSearchGrid.Begin();
                IsSearchVisible = false;
                splitView.IsPaneOpen = false;
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

        private async void shortEstimatedReadingTimeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == shortEstimatedReadingTimeRadioButton)
            {
                ViewModel.LastUsedFilterProperties.MinimumEstimatedReadingTime = 0;
                ViewModel.LastUsedFilterProperties.MaximumEstimatedReadingTime = 5;
            }
            else if (sender == mediumEstimatedReadingTimeRadioButton)
            {
                ViewModel.LastUsedFilterProperties.MinimumEstimatedReadingTime = 5;
                ViewModel.LastUsedFilterProperties.MaximumEstimatedReadingTime = 15;
            }
            else if (sender == longEstimatedReadingTimeRadioButton)
            {
                ViewModel.LastUsedFilterProperties.MinimumEstimatedReadingTime = 15;
                ViewModel.LastUsedFilterProperties.MaximumEstimatedReadingTime = 999; // I really hope there's no article which takes more than 999 minutes to read :D
            }
            await ViewModel.RefreshItemsAsync();
        }
        #endregion

        #region Drag & Drop
        private async void Grid_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.WebLink))
            {
                HideDragDropGridStoryboard.Begin();

                var item = await e.DataView.GetWebLinkAsync();
                await DataService.AddItemAsync(item.ToString());
            }
        }

        private void Grid_DragLeave(object sender, DragEventArgs e) { HideDragDropGridStoryboard.Begin(); }

        private void Grid_DragEnter(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.WebLink))
            {
                ShowDragDropGridStoryboard.Begin();
                e.AcceptedOperation = DataPackageOperation.Move;
            }
        }
        #endregion
    }
}
