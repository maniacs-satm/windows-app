using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        private FilterProperties _lastFilterProperties { get; set; }

        public ObservableCollection<ItemViewModel> Items { get; set; } = new ObservableCollection<ItemViewModel>();
        public ObservableCollection<Tag> Tags { get; set; } = new ObservableCollection<Tag>();
        public ObservableCollection<string> DomainNames { get; set; } = new ObservableCollection<string>();
        public FilterProperties FilterProperties { get; set; } = new FilterProperties();

        #region Tasks & Commands
        public async Task LoadItemsAsync(FilterProperties FilterProperties)
        {
            Items.Clear();
            foreach (Item i in await DataService.GetItemsAsync(FilterProperties))
                Items.Add(new ItemViewModel(i));
        }

        public Command RefreshCommand { get; private set; }
        public Command NavigateToSettingsPageCommand { get; private set; }
        public Command LoadUnreadItemsCommand { get; private set; }
        public Command LoadFavoriteItemsCommand { get; private set; }
        public Command LoadArchivedItemsCommand { get; private set; }
        #endregion

        public override async void OnNavigatedTo(string parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            await LoadItemsAsync(new FilterProperties());
            Tags = new ObservableCollection<Tag>(await DataService.GetTagsAsync());
            foreach (var item in Items)
                if (!DomainNames.Contains(item.Model.DomainName))
                    DomainNames.Add(item.Model.DomainName);

            if (_lastFilterProperties != null)
                await LoadItemsAsync(_lastFilterProperties);

            DomainNames = new ObservableCollection<string>(DomainNames.OrderBy(d => d).ToList());
        }

        public MainViewModel()
        {
            RefreshCommand = new Command(async () =>
            {
                await DataService.SyncWithServerAsync();
                await LoadItemsAsync(new FilterProperties());
                Tags = new ObservableCollection<Tag>(await DataService.GetTagsAsync());
            });
            NavigateToSettingsPageCommand = new Command(() =>
            {
                Services.NavigationService.NavigationService.ApplicationNavigationService.Navigate(typeof(Views.SettingsPage));
            });
            LoadUnreadItemsCommand = new Command(async () =>
            {
                _lastFilterProperties = new FilterProperties();
                await LoadItemsAsync(_lastFilterProperties);
            });
            LoadFavoriteItemsCommand = new Command(async () =>
            {
                _lastFilterProperties = new FilterProperties() { ItemType = FilterProperties.FilterPropertiesItemType.Favorites };
                await LoadItemsAsync(_lastFilterProperties);
            });
            LoadArchivedItemsCommand = new Command(async () =>
            {
                _lastFilterProperties = new FilterProperties() { ItemType = FilterProperties.FilterPropertiesItemType.Archived };
                await LoadItemsAsync(_lastFilterProperties);
            });
        }
    }
}