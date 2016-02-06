using GalaSoft.MvvmLight.Messaging;
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
using wallabag.Data.ViewModels;
using wallabag.Models;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace wallabag.ViewModels
{
    [ImplementPropertyChanged]
    public class MainViewModel : ViewModelBase
    {
        private IDataService _dataService;

        public ObservableCollection<ItemViewModel> Items { get; set; } = new ObservableCollection<ItemViewModel>();
        public ObservableCollection<Tag> Tags { get; set; } = new ObservableCollection<Tag>();
        public ObservableCollection<string> DomainNames { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<OfflineTaskViewModel> OfflineTasks { get; set; } = new ObservableCollection<OfflineTaskViewModel>();

        public ObservableCollection<SearchResult> SearchSuggestions { get; set; } = new ObservableCollection<SearchResult>();
        public ObservableCollection<string> DomainNameSuggestions { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<Tag> TagSuggestions { get; set; } = new ObservableCollection<Tag>();

        public FilterProperties LastUsedFilterProperties { get; set; } = new FilterProperties();
        public bool IsSyncing { get; set; } = false;

        public DelegateCommand RefreshCommand { get; private set; }
        public DelegateCommand AddItemCommand { get; private set; }
        public DelegateCommand NavigateToSettingsPageCommand { get; private set; }
        public DelegateCommand ResetFilterCommand { get; private set; }
        public DelegateCommand<ItemClickEventArgs> ItemClickCommand { get; private set; }

        // Multiple selection
        public bool? IsMultipleSelectionEnabled { get; set; } = false;
        public ObservableCollection<ItemViewModel> SelectedItems { get; set; } = new ObservableCollection<ItemViewModel>();
        public DelegateCommand MarkItemsAsReadCommand { get; private set; }
        public DelegateCommand MarkItemsAsFavoriteCommand { get; private set; }
        public DelegateCommand UnmarkItemsAsReadCommand { get; private set; }
        public DelegateCommand UnmarkItemsAsFavoriteCommand { get; private set; }
        public DelegateCommand DeleteItemsCommand { get; private set; }

        public MainViewModel(IDataService dataService)
        {
            _dataService = dataService;

            RefreshCommand = new DelegateCommand(async () => await RefreshItemsAsync());
            NavigateToSettingsPageCommand = new DelegateCommand(() =>
            {
                NavigationService.Navigate(typeof(Views.SettingsPage));
            });
            AddItemCommand = new DelegateCommand(async () => await Services.DialogService.ShowDialogAsync(Services.DialogService.Dialog.AddItem));

            ResetFilterCommand = new DelegateCommand(async () =>
            {
                LastUsedFilterProperties = new FilterProperties();
                await RefreshItemsAsync();
            });
            ItemClickCommand = new DelegateCommand<ItemClickEventArgs>(args => ItemClick(args));

            MarkItemsAsReadCommand = new DelegateCommand(async () => await MarkItemsAsReadAsync());
            UnmarkItemsAsReadCommand = new DelegateCommand(async () => await UnmarkItemsAsReadAsync());
            MarkItemsAsReadCommand = new DelegateCommand(async () => await MarkItemsAsFavoriteAsync());
            UnmarkItemsAsFavoriteCommand = new DelegateCommand(async () => await UnmarkItemsAsFavoriteAsync());
            DeleteItemsCommand = new DelegateCommand(async () => await DeleteItemsAsync());
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (state.ContainsKey(nameof(LastUsedFilterProperties)))
                LastUsedFilterProperties = JsonConvert.DeserializeObject<FilterProperties>((string)state[nameof(LastUsedFilterProperties)]);
            else
                LastUsedFilterProperties = new FilterProperties();

            await LoadItemsFromDatabaseAsync();
            
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

            Messenger.Default.Register<NotificationMessage<Uri>>(this, async uri =>
            {
                await _dataService.AddItemAsync(uri.Content.ToString());
                await RefreshItemsAsync();
            });
        }

        private async Task GetOfflineTasksAsync()
        {
            OfflineTasks.Clear();
            foreach (var item in await _dataService.GetOfflineTasksAsync())
            {
                var viewModel = new OfflineTaskViewModel(_dataService);
                await viewModel.InitializeAsync(item);

                OfflineTasks.Add(viewModel);                
            }
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            state[nameof(LastUsedFilterProperties)] = JsonConvert.SerializeObject(LastUsedFilterProperties);

            Messenger.Default.Unregister(this);
            return base.OnNavigatedFromAsync(state, suspending);
        }

        public void ItemClick(ItemClickEventArgs e)
        {
            if (e.ClickedItem != null)
            {
                var clickedItem = (ItemViewModel)e.ClickedItem;
                NavigationService.Navigate(typeof(Views.SingleItemPage), clickedItem.Model.Id);
            }
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

                    await _dataService.UpdateItemAsync(item);
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

            await GetOfflineTasksAsync();
        }

        public async Task MarkItemsAsReadAsync()
        {
            foreach (var item in SelectedItems)
            {
                item.Model.IsRead = true;
                await ItemViewModel.UpdateSpecificProperty(item.Model.Id, "archive", item.Model.IsRead);
            }
            await FinishMultipleSelection();
        }
        public async Task UnmarkItemsAsReadAsync()
        {
            foreach (var item in SelectedItems)
            {
                item.Model.IsRead = false;
                await ItemViewModel.UpdateSpecificProperty(item.Model.Id, "archive", item.Model.IsRead);
            }
            await FinishMultipleSelection();
        }
        public async Task MarkItemsAsFavoriteAsync()
        {
            foreach (var item in SelectedItems)
            {
                item.Model.IsStarred = true;
                await ItemViewModel.UpdateSpecificProperty(item.Model.Id, "star", item.Model.IsStarred);
            }
            await FinishMultipleSelection();
        }
        public async Task UnmarkItemsAsFavoriteAsync()
        {
            foreach (var item in SelectedItems)
            {
                item.Model.IsStarred = false;
                await ItemViewModel.UpdateSpecificProperty(item.Model.Id, "star", item.Model.IsStarred);
            }
            await FinishMultipleSelection();
        }
        public async Task DeleteItemsAsync()
        {
            foreach (var item in SelectedItems)
            {
                item.Model.IsDeleted = true;
                await item.DeleteAsync();
            }
            await FinishMultipleSelection();
        }
        private async Task FinishMultipleSelection()
        {
            foreach (var item in SelectedItems)
                await _dataService.UpdateItemAsync(item.Model);

            await RefreshItemsAsync();
            IsMultipleSelectionEnabled = false;
        }
    }
}