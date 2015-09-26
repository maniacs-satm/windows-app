﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using wallabag.Common;
using wallabag.Models;
using wallabag.Services;
using wallabag.ViewModels;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace wallabag.Views
{
    public sealed partial class ContentPage : Page
    {
        public MainViewModel ViewModel { get { return (MainViewModel)DataContext; } }

        #region Context menu
        private bool _IsShiftPressed = false;
        private bool _IsPointerPressed = false;

        protected override void OnKeyDown(KeyRoutedEventArgs e)
        {
            // Handle Shift+F10
            // Handle MenuKey

            if (e.Key == Windows.System.VirtualKey.Shift)
            {
                _IsShiftPressed = true;
            }

            // Shift+F10
            else if (_IsShiftPressed && e.Key == Windows.System.VirtualKey.F10)
            {
                var FocusedUIElement = FocusManager.GetFocusedElement() as UIElement;

                ItemViewModel FocusedItem = null;
                if (FocusedUIElement is ContentControl)
                {
                    FocusedItem = ((ContentControl)FocusedUIElement).Content as ItemViewModel;
                }
                ShowContextMenu(FocusedItem, FocusedUIElement, new Point(0, 0));
                e.Handled = true;
            }

            // The 'Menu' key next to Right Ctrl on most keyboards
            else if (e.Key == Windows.System.VirtualKey.Application)
            {
                var FocusedUIElement = FocusManager.GetFocusedElement() as UIElement;
                ItemViewModel FocusedItem = null;
                if (FocusedUIElement is ContentControl)
                {
                    FocusedItem = ((ContentControl)FocusedUIElement).Content as ItemViewModel;
                }
                ShowContextMenu(FocusedItem, FocusedUIElement, new Point(0, 0));
                e.Handled = true;
            }

            base.OnKeyDown(e);
        }
        protected override void OnKeyUp(KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Shift)
                _IsShiftPressed = false;

            base.OnKeyUp(e);
        }
        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            _IsPointerPressed = true;

            base.OnPointerPressed(e);
        }
        protected override void OnRightTapped(RightTappedRoutedEventArgs e)
        {
            base.OnRightTapped(e);
        }
        private void ShowContextMenu(ItemViewModel data, UIElement target, Point offset)
        {
            var MyFlyout = Resources["ItemContextMenu"] as MenuFlyout;
            MyFlyout.ShowAt(target, offset);
        }
        #endregion

        public ObservableCollection<KeyValuePair<int, string>> PossibleSearchBoxResults { get; set; } = new ObservableCollection<KeyValuePair<int, string>>();
        public ObservableCollection<KeyValuePair<int, string>> SearchBoxSuggestions { get; set; } = new ObservableCollection<KeyValuePair<int, string>>();

        public ObservableCollection<Tag> MultipleSelectionTags { get; set; }

        public ContentPage()
        {
            InitializeComponent();
            AddItemContentDialog.Closed += AddItemContentDialog_Closed;
        }

        private async void AddItemContentDialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            await ViewModel.LoadItemsAsync();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            List<Item> allItems = await DataService.GetItemsAsync(new FilterProperties() { ItemType = FilterProperties.FilterPropertiesItemType.All });
            foreach (var item in allItems)
                PossibleSearchBoxResults.Add(new KeyValuePair<int, string>(item.Id, item.Title));
        }

        private void ItemGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var clickedItem = (ItemViewModel)e.ClickedItem;
            Services.NavigationService.NavigationService.ApplicationNavigationService.Navigate(typeof(SingleItemPage), clickedItem.Model.Id.ToString());
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
                Services.NavigationService.NavigationService.ApplicationNavigationService.Navigate(typeof(SingleItemPage), id.ToString());
            }
            // TODO: Implement a search page in case the user didn't chose a suggestion.
        }

        private async void AddItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (Window.Current.Bounds.Width <= 500 || Helpers.IsPhone)
                Services.NavigationService.NavigationService.ApplicationNavigationService.Navigate(typeof(AddItemPage));
            else
                await AddItemContentDialog.ShowAsync();
        }

        private void multipleSelectToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            //ItemGridView.SelectionMode = ListViewSelectionMode.Multiple;
            //ItemListView.SelectionMode = ListViewSelectionMode.Multiple;
            acceptAppBarButton.Visibility = Visibility.Visible;
            favoriteAppBarButton.Visibility = Visibility.Visible;
            tagAppBarButton.Visibility = Visibility.Visible;
            deleteAppBarButton.Visibility = Visibility.Visible;
            filterToggleButton.Visibility = Visibility.Collapsed;
            addItemAppBarButton.Visibility = Visibility.Collapsed;
            syncAppBarButton.Visibility = Visibility.Collapsed;
        }
        private void multipleSelectToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            //ItemGridView.SelectionMode = ListViewSelectionMode.None;
            //ItemListView.SelectionMode = ListViewSelectionMode.None;
            acceptAppBarButton.Visibility = Visibility.Collapsed;
            favoriteAppBarButton.Visibility = Visibility.Collapsed;
            tagAppBarButton.Visibility = Visibility.Collapsed;
            deleteAppBarButton.Visibility = Visibility.Collapsed;
            filterToggleButton.Visibility = Visibility.Visible;
            addItemAppBarButton.Visibility = Visibility.Visible;
            syncAppBarButton.Visibility = Visibility.Visible;
        }

        private async void acceptAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            //foreach (ItemViewModel item in ItemGridView.SelectedItems)
            //    await item.SwitchReadValueAsync();
        }

        private async void favoriteAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            //foreach (ItemViewModel item in ItemGridView.SelectedItems)
            //    await item.SwitchFavoriteValueAsync();
        }

        private async void deleteAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            //foreach (ItemViewModel item in ItemGridView.SelectedItems)
            //    await item.DeleteItemAsync();
        }

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
    }
}
