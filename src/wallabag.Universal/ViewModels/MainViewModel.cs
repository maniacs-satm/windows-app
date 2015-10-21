using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PropertyChanged;
using Template10.Mvvm;
using wallabag.Common;
using wallabag.Models;
using wallabag.Services;
using Windows.UI.Xaml.Navigation;

namespace wallabag.ViewModels
{
    [ImplementPropertyChanged]
    public class MainViewModel : ViewModelBase
    {
        public DateTimeOffset MaxDate { get; } = DateTimeOffset.Now;

        public ObservableCollection<ItemViewModel> Items { get; set; } = new ObservableCollection<ItemViewModel>();
        public ObservableCollection<Tag> Tags { get; set; } = new ObservableCollection<Tag>();
        public ObservableCollection<string> DomainNames { get; set; } = new ObservableCollection<string>();
        public FilterProperties LastUsedFilterProperties { get; set; } = new FilterProperties();

        public bool IsSyncing { get; set; } = false;
        public int NumberOfOfflineActions { get; set; }

        private ObservableCollection<Item> _Models { get; set; } = new ObservableCollection<Item>();

        #region Tasks & Commands
        public async Task LoadItemsAsync()
        {
            bool sortDescending = LastUsedFilterProperties.SortOrder == FilterProperties.FilterPropertiesSortOrder.Descending;

            foreach (Item i in await DataService.GetItemsAsync(LastUsedFilterProperties))
            {
                if (!_Models.Contains(i))
                {
                    _Models.Add(i);
                    Items.AddSorted(new ItemViewModel(i), new ItemByDateTimeComparer(), sortDescending);
                }
            }
            await RefreshItemsAsync(true);
        }
        public async Task RefreshItemsAsync(bool firstStart = false)
        {
            bool sortDescending = LastUsedFilterProperties.SortOrder == FilterProperties.FilterPropertiesSortOrder.Descending;

            if (!firstStart)
            {
                var itemsInDatabase = await DataService.GetItemsAsync(LastUsedFilterProperties);

                var newItems = itemsInDatabase.Except(_Models, new ItemComparer()).ToList();
                var removedItems = _Models.Except(itemsInDatabase, new ItemComparer()).ToList();

                foreach (var item in newItems)
                {
                    Items.AddSorted(new ItemViewModel(item), new ItemByDateTimeComparer(), sortDescending);
                    _Models.Add(item);
                }
                foreach (var item in removedItems)
                {
                    ItemViewModel itemInCollection = Items.Where(i => i.Model == item).First();
                    Items.Remove(itemInCollection);
                    _Models.Remove(item);
                }
            }

            Tags = new ObservableCollection<Tag>(await DataService.GetTagsAsync());
            foreach (var item in Items)
                if (!DomainNames.Contains(item.Model.DomainName))
                    DomainNames.Add(item.Model.DomainName);

            DomainNames = new ObservableCollection<string>(DomainNames.OrderBy(d => d));
        }

        public DelegateCommand RefreshCommand { get; private set; }
        public DelegateCommand NavigateToSettingsPageCommand { get; private set; }

        public DelegateCommand ResetFilterCommand { get; private set; }
        #endregion

        public override async void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (state.ContainsKey(nameof(LastUsedFilterProperties)))
                LastUsedFilterProperties = JsonConvert.DeserializeObject<FilterProperties>((string)state[nameof(LastUsedFilterProperties)]);
            else
                LastUsedFilterProperties = new FilterProperties();

            await LoadItemsAsync();

            SQLite.SQLiteAsyncConnection conn = new SQLite.SQLiteAsyncConnection(Helpers.DATABASE_PATH);
            NumberOfOfflineActions = await conn.Table<OfflineTask>().CountAsync();

            if (AppSettings.SyncOnStartup)
                RefreshCommand.Execute(null);
        }
        public override Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            state[nameof(LastUsedFilterProperties)] = JsonConvert.SerializeObject(LastUsedFilterProperties);
            return base.OnNavigatedFromAsync(state, suspending);
        }

        public MainViewModel()
        {
            RefreshCommand = new DelegateCommand(async () =>
            {
                IsSyncing = true;
                await DataService.SyncOfflineTasksWithServerAsync();
                await DataService.DownloadItemsFromServerAsync();
                DataService.LastUserSyncDateTime = DateTime.Now;
                await RefreshItemsAsync();
                IsSyncing = false;

                SQLite.SQLiteAsyncConnection conn = new SQLite.SQLiteAsyncConnection(Helpers.DATABASE_PATH);
                NumberOfOfflineActions = await conn.Table<OfflineTask>().CountAsync();
            });
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
    }
}