using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using PropertyChanged;
using wallabag.Common.Mvvm;
using wallabag.Models;
using wallabag.Services;
using Windows.UI.Xaml.Navigation;

namespace wallabag.ViewModels
{
    [ImplementPropertyChanged]
    public class MainViewModel : ViewModelBase
    {
        public override string ViewModelIdentifier { get; set; } = "MainViewModel";

        public ObservableCollection<ItemViewModel> Items { get; set; } = new ObservableCollection<ItemViewModel>();
        public ObservableCollection<Tag> Tags { get; set; } = new ObservableCollection<Tag>();
        public bool IsHome { get; set; } = true;

        #region Tasks & Commands
        private async Task LoadItemsAsync(FilterProperties FilterProperties)
        {
            Items.Clear();
            foreach (Item i in await DataSource.GetItemsAsync(FilterProperties))
                Items.Add(new ItemViewModel(i));
        }

        public Command RefreshCommand { get; private set; }
        public Command NavigateToSettingsPageCommand { get; private set; }
        public Command LoadUnreadItemsCommand { get; private set; }
        public Command LoadFavoriteItemsCommand { get; private set; }
        #endregion

        public override async void OnNavigatedTo(string parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            await LoadItemsAsync(new FilterProperties());
            Tags = new ObservableCollection<Tag>(await DataSource.GetTagsAsync());
        }

        public MainViewModel()
        {
            RefreshCommand = new Command(async () =>
            {
                await DataSource.DownloadItemsFromServerAsync();
                await LoadItemsAsync(new FilterProperties());
                Tags = new ObservableCollection<Tag>(await DataSource.GetTagsAsync());
            });
            NavigateToSettingsPageCommand = new Command(() =>
            {
                Services.NavigationService.NavigationService.ApplicationNavigationService.Navigate(typeof(Views.SettingsPage));
            });
            LoadUnreadItemsCommand = new Command(async () =>
            {
                await LoadItemsAsync(new FilterProperties());
                IsHome = true;
            });
            LoadFavoriteItemsCommand = new Command(async () =>
           {
               await LoadItemsAsync(new FilterProperties() { itemType = FilterProperties.ItemType.Favorites });
               IsHome = false;
           });
        }
    }
}