using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Template10.Mvvm;
using wallabag.Common;
using wallabag.Data.Interfaces;
using wallabag.Data.Models;
using wallabag.Models;
using Windows.UI.Xaml.Navigation;

namespace wallabag.ViewModels
{
    [ImplementPropertyChanged]
    public class MainViewModel : ViewModelBase
    {
        private IDataService _dataService;
        private SQLite.SQLiteAsyncConnection _sqlconn;

        public ObservableCollection<ItemViewModel> Items { get; set; }
        public ObservableCollection<Tag> Tags { get; set; }
        public ObservableCollection<string> DomainNames { get; set; }

        public ObservableCollection<SearchResult> SearchSuggestions { get; set; }
        public ObservableCollection<string> DomainNameSuggestions { get; set; }
        public ObservableCollection<Tag> TagSuggestions { get; set; }

        public FilterProperties LastUsedFilterProperties { get; set; } = new FilterProperties();
        public bool IsSyncing { get; set; } = false;
        public int NumberOfOfflineTasks { get; set; } = 0;

        public DelegateCommand RefreshCommand { get; private set; }
        public DelegateCommand NavigateToSettingsPageCommand { get; private set; }
        public DelegateCommand ResetFilterCommand { get; private set; }

        public MainViewModel(IDataService dataService)
        {
            _dataService = dataService;
            _sqlconn = new SQLite.SQLiteAsyncConnection(Helpers.DATABASE_PATH);

            RefreshCommand = new DelegateCommand(async () => await RefreshItemsAsync());
            NavigateToSettingsPageCommand = new DelegateCommand(() =>
            {
                NavigationService.Navigate(typeof(Views.SettingsPage));
            });

            ResetFilterCommand = new DelegateCommand(async () =>
            {
                LastUsedFilterProperties = new FilterProperties();
                await RefreshItemsAsync();
            });
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (state.ContainsKey(nameof(LastUsedFilterProperties)))
                LastUsedFilterProperties = JsonConvert.DeserializeObject<FilterProperties>((string)state[nameof(LastUsedFilterProperties)]);
            else
                LastUsedFilterProperties = new FilterProperties();

            await LoadItemsFromDatabaseAsync();

            NumberOfOfflineTasks = await _sqlconn.Table<OfflineTask>().CountAsync();

            if (AppSettings.SyncOnStartup)
                await RefreshItemsAsync();

            List<Item> allItems = await _dataService.GetItemsAsync(new FilterProperties() { ItemType = FilterProperties.FilterPropertiesItemType.All });
            foreach (var item in allItems)
            {
                SearchSuggestions.Add(new SearchResult(item.Id, item.Title));

                string domainName = item.DomainName.Replace("www.", string.Empty);
                if (!DomainNames.Contains(domainName))
                    DomainNames.Add(domainName);

                foreach (Tag tag in item.Tags)
                    if (!Tags.Contains(tag))
                        Tags.Add(tag);
            }
        }
        public override Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            state[nameof(LastUsedFilterProperties)] = JsonConvert.SerializeObject(LastUsedFilterProperties);
            return base.OnNavigatedFromAsync(state, suspending);
        }

        public async Task RefreshItemsAsync()
        {
            IsSyncing = true;

            await _dataService.SyncOfflineTasksWithServerAsync();
            await _dataService.DownloadItemsFromServerAsync();

            bool firstStart = Items.Count == 0;
            await LoadItemsFromDatabaseAsync(firstStart);

            IsSyncing = false;
        }
        public async Task LoadItemsFromDatabaseAsync(bool firstStart = false, bool completeReorder = false)
        {
            bool sortDescending = LastUsedFilterProperties.SortOrder == FilterProperties.FilterPropertiesSortOrder.Descending;

            if (!firstStart)
            {
                var itemsInDatabase = await _dataService.GetItemsAsync(LastUsedFilterProperties);
                var currentItems = new List<Item>();

                foreach (var item in Items)
                    currentItems.Add(item.Model);

                var newItems = itemsInDatabase.Except(currentItems).ToList();
                var changedItems = itemsInDatabase.Except(currentItems, new ItemChangedComparer()).ToList();
                var removedItems = currentItems.Except(itemsInDatabase).ToList();

                foreach (var item in newItems)
                    Items.AddSorted(new ItemViewModel(item), new ItemByDateTimeComparer(), sortDescending);

                foreach (var item in changedItems)
                {
                    Items.Remove(Items.Where(i => i.Model.Id == item.Id).First());
                    Items.AddSorted(new ItemViewModel(item), new ItemByDateTimeComparer(), sortDescending);

                    await _sqlconn.UpdateAsync(item);
                }

                foreach (var item in removedItems)
                    Items.Remove(new ItemViewModel(item));
            }

            Tags = new ObservableCollection<Tag>(await _dataService.GetTagsAsync());

            foreach (var item in Items)
                if (!DomainNames.Contains(item.Model.DomainName))
                    DomainNames.Add(item.Model.DomainName);

            DomainNames = new ObservableCollection<string>(DomainNames.OrderBy(d => d));

            if (completeReorder)
            {
                if (LastUsedFilterProperties.SortOrder == FilterProperties.FilterPropertiesSortOrder.Ascending)
                    Items = new ObservableCollection<ItemViewModel>(Items.OrderBy(i => i.Model.CreationDate));
                else
                    Items = new ObservableCollection<ItemViewModel>(Items.OrderByDescending(i => i.Model.CreationDate));
            }

            NumberOfOfflineTasks = await _sqlconn.Table<OfflineTask>().CountAsync();
        }
    }
}