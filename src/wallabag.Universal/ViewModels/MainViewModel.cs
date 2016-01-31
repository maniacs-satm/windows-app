﻿using Newtonsoft.Json;
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

        public DateTimeOffset MaxDate { get; } = DateTimeOffset.Now;

        public ObservableCollection<ItemViewModel> Items { get; set; } = new ObservableCollection<ItemViewModel>();
        public ObservableCollection<Tag> Tags { get; set; } = new ObservableCollection<Tag>();
        public ObservableCollection<string> DomainNames { get; set; } = new ObservableCollection<string>();
        public FilterProperties LastUsedFilterProperties { get; set; } = new FilterProperties();

        public bool IsSyncing { get; set; } = false;
        public int NumberOfOfflineTasks { get; set; }

        private ObservableCollection<Item> _Models { get; set; } = new ObservableCollection<Item>();

        #region Tasks & Commands
        public async Task LoadItemsAsync()
        {
            bool sortDescending = LastUsedFilterProperties.SortOrder == FilterProperties.FilterPropertiesSortOrder.Descending;

            foreach (Item i in await _dataService.GetItemsAsync(LastUsedFilterProperties))
            {
                if (_Models.Contains(i, new ItemComparer()) == false)
                {
                    _Models.Add(i);
                    Items.AddSorted(new ItemViewModel(i), new ItemByDateTimeComparer(), sortDescending);
                }
            }
            await RefreshItemsAsync(true);
        }
        public async Task RefreshItemsAsync(bool firstStart = false, bool completeReorder = false)
        {
            bool sortDescending = LastUsedFilterProperties.SortOrder == FilterProperties.FilterPropertiesSortOrder.Descending;

            if (!firstStart)
            {
                var itemsInDatabase = await _dataService.GetItemsAsync(LastUsedFilterProperties);

                var newItems = itemsInDatabase.Except(_Models, new ItemComparer()).ToList();
                var changedItems = itemsInDatabase.Except(_Models, new ItemChangedComparer()).ToList();
                var removedItems = _Models.Except(itemsInDatabase, new ItemComparer()).ToList();

                foreach (var item in newItems)
                {
                    Items.AddSorted(new ItemViewModel(item), new ItemByDateTimeComparer(), sortDescending);
                    _Models.Add(item);
                }
                foreach (var item in changedItems)
                {
                    Items.Remove(Items.Where(i => i.Model.Id == item.Id).First());
                    Items.AddSorted(new ItemViewModel(item), new ItemByDateTimeComparer(), sortDescending);
                    _Models.Remove(_Models.Where(i => i.Id == item.Id).First());
                    _Models.Add(item);

                    await new SQLite.SQLiteAsyncConnection(Helpers.DATABASE_PATH).UpdateAsync(item);
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

            if (completeReorder)
            {
                if (LastUsedFilterProperties.SortOrder == FilterProperties.FilterPropertiesSortOrder.Ascending)
                    Items = new ObservableCollection<ItemViewModel>(Items.OrderBy(i => i.Model.CreationDate));
                else
                    Items = new ObservableCollection<ItemViewModel>(Items.OrderByDescending(i => i.Model.CreationDate));

            }

            SQLite.SQLiteAsyncConnection conn = new SQLite.SQLiteAsyncConnection(Helpers.DATABASE_PATH);
            NumberOfOfflineTasks = await conn.Table<OfflineTask>().CountAsync();
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
            NumberOfOfflineTasks = await conn.Table<OfflineTask>().CountAsync();

            if (AppSettings.SyncOnStartup)
                RefreshCommand.Execute(null);
        }
        public override Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            state[nameof(LastUsedFilterProperties)] = JsonConvert.SerializeObject(LastUsedFilterProperties);
            return base.OnNavigatedFromAsync(state, suspending);
        }

        public MainViewModel(IDataService dataService)
        {
            _dataService = dataService;
            RefreshCommand = new DelegateCommand(async () =>
            {
                IsSyncing = true;

                await _dataService.SyncOfflineTasksWithServerAsync();
                await _dataService.DownloadItemsFromServerAsync();
                await RefreshItemsAsync();

                IsSyncing = false;
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