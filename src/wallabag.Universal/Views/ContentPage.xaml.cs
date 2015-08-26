using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using wallabag.Common;
using wallabag.Models;
using wallabag.Services;
using wallabag.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace wallabag.Views
{
    public sealed partial class ContentPage : Page
    {
        public MainViewModel ViewModel { get { return (MainViewModel)DataContext; } }
        public ObservableCollection<string> SearchBoxSuggestions { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> TitleList { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<Tag> MultipleSelectionTags { get; set; }

        public ContentPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            List<Models.Item> allItems= await DataService.GetItemsAsync(new FilterProperties() { ItemType = FilterProperties.FilterPropertiesItemType.All});
            foreach (var item in allItems)
                TitleList.Add(item.Title);
        }

        private void ItemGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var clickedItem = (ItemViewModel)e.ClickedItem;
            Services.NavigationService.NavigationService.ApplicationNavigationService.Navigate(typeof(SingleItemPage), clickedItem.Model.Id.ToString());
        }

        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var possibleResults = new ObservableCollection<string>(TitleList.Where(t=>t.ToLower().Contains(sender.Text.ToLower())));

            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                SearchBoxSuggestions.Clear();
                foreach (var item in possibleResults)
                    SearchBoxSuggestions.Add(item);
            }
        }

        private async void SearchBox_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            // TODO: It's not working atm.
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var id = (await DataService.GetItemAsync((sender as AutoSuggestBox).Text)).Id;
                Services.NavigationService.NavigationService.ApplicationNavigationService.Navigate(typeof(SingleItemPage), id.ToString());
            }
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
            ItemGridView.SelectionMode = ListViewSelectionMode.Multiple;
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
            ItemGridView.SelectionMode = ListViewSelectionMode.None;
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
            foreach (ItemViewModel item in ItemGridView.SelectedItems)
                await item.SwitchReadValueAsync();
        }

        private async void favoriteAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (ItemViewModel item in ItemGridView.SelectedItems)
                await item.SwitchFavoriteValueAsync();
        }

        private async void deleteAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (ItemViewModel item in ItemGridView.SelectedItems)
                await item.DeleteItemAsync();
        }
    }
}
