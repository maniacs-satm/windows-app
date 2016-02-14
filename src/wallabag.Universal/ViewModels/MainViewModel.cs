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
using Windows.UI.Xaml.Media.Animation;
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

        public FilterProperties CurrentFilterProperties { get; set; } = new FilterProperties();
        public bool IsSyncing { get; set; } = false;

        public DelegateCommand RefreshCommand { get; private set; }
        public DelegateCommand AddItemCommand { get; private set; }
        public DelegateCommand NavigateToSettingsPageCommand { get; private set; }
        public DelegateCommand<ItemClickEventArgs> ItemClickCommand { get; private set; }

        // Multiple selection
        public bool? IsMultipleSelectionEnabled { get; set; } = false;
        public ObservableCollection<ItemViewModel> SelectedItems { get; set; } = new ObservableCollection<ItemViewModel>();
        public DelegateCommand MarkItemsAsReadCommand { get; private set; }
        public DelegateCommand MarkItemsAsFavoriteCommand { get; private set; }
        public DelegateCommand UnmarkItemsAsReadCommand { get; private set; }
        public DelegateCommand UnmarkItemsAsFavoriteCommand { get; private set; }
        public DelegateCommand DeleteItemsCommand { get; private set; }

        // Search
        public bool? IsItemClickEnabled { get; set; } = true;
        public bool IsSearchVisible { get; set; } = false;
        public string SearchQuery { get; set; }
        public DelegateCommand<AutoSuggestBoxTextChangedEventArgs> SearchQueryChangedCommand { get; private set; }
        public DelegateCommand<AutoSuggestBoxQuerySubmittedEventArgs> SearchQuerySubmittedCommand { get; private set; }

        // Filter
        public enum FilterSortType { ByDate, ByTitle }
        public enum FilterSortOrder { Ascending, Descending }
        public enum FilterEstimatedReadingTime { Unfiltered, Short, Medium, Long }
        public FilterSortType SortType { get; set; } = FilterSortType.ByDate;
        public FilterSortOrder SortOrder { get; set; } = FilterSortOrder.Descending;
        public FilterEstimatedReadingTime EstimatedReadingTime { get; set; } = FilterEstimatedReadingTime.Unfiltered;
        private DateTimeOffset _MinDate = DateTimeOffset.Now;
        private DateTimeOffset _MaxDate = DateTimeOffset.Now;
        public DelegateCommand<string> ItemTypeSelectionChangedCommand { get; set; }
        public DelegateCommand<string> ItemSortOrderChangedCommand { get; set; }
        public DelegateCommand<string> ItemSortTypeChangedCommand { get; set; }
        public string DomainQuery { get; set; }
        public DelegateCommand<AutoSuggestBoxTextChangedEventArgs> DomainQueryChangedCommand { get; private set; }
        public DelegateCommand<AutoSuggestBoxQuerySubmittedEventArgs> DomainQuerySubmittedCommand { get; private set; }
        public string TagQuery { get; set; }
        public DelegateCommand<AutoSuggestBoxTextChangedEventArgs> TagQueryChangedCommand { get; private set; }
        public DelegateCommand<AutoSuggestBoxQuerySubmittedEventArgs> TagQuerySubmittedCommand { get; private set; }
        public DelegateCommand<string> EstimatedReadingTimeFilterChangedCommand { get; private set; }
        public DateTimeOffset? MinDateNullable
        {
            get { return _MinDate; }
            set
            {
                _MinDate = (DateTimeOffset)value;
                RaisePropertyChanged(nameof(MinDateNullable));
                RaisePropertyChanged(nameof(MinDate));
            }
        }
        public DateTimeOffset MinDate { get { return _MinDate; } }
        public DateTimeOffset? MaxDateNullable
        {
            get { return _MaxDate; }
            set
            {
                _MaxDate = value ?? DateTimeOffset.Now;
                RaisePropertyChanged(nameof(MaxDateNullable));
            }
        }
        public DateTimeOffset MaxDate { get; } = DateTimeOffset.Now;
        public DelegateCommand<CalendarDatePickerDateChangedEventArgs> CreationDateFilterChangedCommand { get; private set; }
        public DelegateCommand ResetFilterCommand { get; private set; }
        public DelegateCommand FilterCommand { get; private set; }

        public MainViewModel(IDataService dataService)
        {
            _dataService = dataService;

            RefreshCommand = new DelegateCommand(async () => await RefreshItemsAsync());
            NavigateToSettingsPageCommand = new DelegateCommand(() =>
            {
                NavigationService.Navigate(typeof(Views.SettingsPage), null, new DrillInNavigationTransitionInfo());
            });
            AddItemCommand = new DelegateCommand(async () => await Services.DialogService.ShowDialogAsync(Services.DialogService.Dialog.AddItem));

            ResetFilterCommand = new DelegateCommand(async () =>
            {
                CurrentFilterProperties = new FilterProperties();
                SortType = FilterSortType.ByDate;
                SortOrder = FilterSortOrder.Descending;
                EstimatedReadingTime = FilterEstimatedReadingTime.Unfiltered;
                await LoadItemsFromDatabaseAsync();
            });
            FilterCommand = new DelegateCommand(async () =>
            {
                await LoadItemsFromDatabaseAsync(true);
            });
            ItemClickCommand = new DelegateCommand<ItemClickEventArgs>(args => ItemClick(args));

            MarkItemsAsReadCommand = new DelegateCommand(async () => await MarkItemsAsReadAsync());
            UnmarkItemsAsReadCommand = new DelegateCommand(async () => await UnmarkItemsAsReadAsync());
            MarkItemsAsReadCommand = new DelegateCommand(async () => await MarkItemsAsFavoriteAsync());
            UnmarkItemsAsFavoriteCommand = new DelegateCommand(async () => await UnmarkItemsAsFavoriteAsync());
            DeleteItemsCommand = new DelegateCommand(async () => await DeleteItemsAsync());

            ItemTypeSelectionChangedCommand = new DelegateCommand<string>(itemType => ItemTypeSelectionChanged(itemType));
            ItemSortOrderChangedCommand = new DelegateCommand<string>(sortOrder => ItemSortOrderChanged(sortOrder));
            ItemSortTypeChangedCommand = new DelegateCommand<string>(sortType => ItemSortTypeChanged(sortType));
            SearchQueryChangedCommand = new DelegateCommand<AutoSuggestBoxTextChangedEventArgs>(async e => await SearchQueryChangedAsync(e));
            SearchQuerySubmittedCommand = new DelegateCommand<AutoSuggestBoxQuerySubmittedEventArgs>(e => SearchQuerySubmitted(e));
            DomainQueryChangedCommand = new DelegateCommand<AutoSuggestBoxTextChangedEventArgs>(e => DomainQueryChanged(e));
            DomainQuerySubmittedCommand = new DelegateCommand<AutoSuggestBoxQuerySubmittedEventArgs>(e => DomainQuerySubmitted(e));
            TagQueryChangedCommand = new DelegateCommand<AutoSuggestBoxTextChangedEventArgs>(e => TagQueryChanged(e));
            TagQuerySubmittedCommand = new DelegateCommand<AutoSuggestBoxQuerySubmittedEventArgs>(e => TagQuerySubmitted(e));
            EstimatedReadingTimeFilterChangedCommand = new DelegateCommand<string>(readingTime => EstimatedReadingTimeFilterChanged(readingTime));
            CreationDateFilterChangedCommand = new DelegateCommand<CalendarDatePickerDateChangedEventArgs>(args => CreationDateFilterChanged(args));
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (state.ContainsKey(nameof(CurrentFilterProperties)))
                CurrentFilterProperties = JsonConvert.DeserializeObject<FilterProperties>((string)state[nameof(CurrentFilterProperties)]);
            else
                CurrentFilterProperties = new FilterProperties();

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
            Messenger.Default.Register<NotificationMessage<bool>>(this, message =>
            {
                if (message.Notification == "SetItemClickEnabled")
                    IsItemClickEnabled = message.Content;
                else if (message.Notification == "SetMultipleSelectionEnabled")
                    IsMultipleSelectionEnabled = message.Content;
            });
            Messenger.Default.Register<NotificationMessage>(this, async message =>
            {
                if (message.Notification == "UpdateView")
                    await LoadItemsFromDatabaseAsync(true);
            });
        }
        public override Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            state[nameof(CurrentFilterProperties)] = JsonConvert.SerializeObject(CurrentFilterProperties);

            Messenger.Default.Unregister(this);
            return base.OnNavigatedFromAsync(state, suspending);
        }

        public void ItemClick(ItemClickEventArgs e)
        {
            if (e.ClickedItem != null && IsItemClickEnabled == true)
            {
                var clickedItem = (ItemViewModel)e.ClickedItem;
                NavigationService.Navigate(typeof(Views.SingleItemPage), clickedItem.Model.Id, new DrillInNavigationTransitionInfo());
            }
        }

        public async Task RefreshItemsAsync()
        {
            IsSyncing = true;

            CurrentFilterProperties.SearchQuery = string.Empty;

            await _dataService.SyncOfflineTasksWithServerAsync();
            await _dataService.DownloadItemsFromServerAsync();

            await LoadItemsFromDatabaseAsync();

            IsSyncing = false;
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
        public async Task LoadItemsFromDatabaseAsync(bool completeReorder = false)
        {
            bool sortDescending = SortOrder == FilterSortOrder.Descending;

            var itemsInDatabase = await _dataService.GetItemsAsync(CurrentFilterProperties);
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

            Tags = new ObservableCollection<Tag>(await _dataService.GetTagsAsync());

            foreach (var item in Items)
                if (!DomainNames.Contains(item.Model.DomainName))
                    DomainNames.Add(item.Model.DomainName);

            DomainNames.Sort(d => d);

            if (completeReorder)
                SortItems();

            await GetOfflineTasksAsync();
        }

        public async Task MarkItemsAsReadAsync()
        {
            foreach (var item in SelectedItems)
            {
                item.Model.IsRead = true;
                await ItemViewModel.UpdateSpecificPropertyAsync(item.Model.Id, ItemViewModel.ItemReadAPIString, item.Model.IsRead);
            }
            await FinishMultipleSelection();
        }
        public async Task UnmarkItemsAsReadAsync()
        {
            foreach (var item in SelectedItems)
            {
                item.Model.IsRead = false;
                await ItemViewModel.UpdateSpecificPropertyAsync(item.Model.Id, ItemViewModel.ItemReadAPIString, item.Model.IsRead);
            }
            await FinishMultipleSelection();
        }
        public async Task MarkItemsAsFavoriteAsync()
        {
            foreach (var item in SelectedItems)
            {
                item.Model.IsStarred = true;
                await ItemViewModel.UpdateSpecificPropertyAsync(item.Model.Id, ItemViewModel.ItemStarredAPIString, item.Model.IsStarred);
            }
            await FinishMultipleSelection();
        }
        public async Task UnmarkItemsAsFavoriteAsync()
        {
            foreach (var item in SelectedItems)
            {
                item.Model.IsStarred = false;
                await ItemViewModel.UpdateSpecificPropertyAsync(item.Model.Id, ItemViewModel.ItemStarredAPIString, item.Model.IsStarred);
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
            IsItemClickEnabled = false;
        }

        public void ItemTypeSelectionChanged(string itemType)
        {
            switch (itemType)
            {
                case "all":
                    CurrentFilterProperties.ItemType = FilterProperties.FilterPropertiesItemType.All;
                    break;
                case "unread":
                    CurrentFilterProperties.ItemType = FilterProperties.FilterPropertiesItemType.Unread;
                    break;
                case "starred":
                    CurrentFilterProperties.ItemType = FilterProperties.FilterPropertiesItemType.Favorites;
                    break;
                case "archived":
                    CurrentFilterProperties.ItemType = FilterProperties.FilterPropertiesItemType.Archived;
                    break;
                case "deleted":
                    CurrentFilterProperties.ItemType = FilterProperties.FilterPropertiesItemType.Deleted;
                    break;
            }
        }
        public void ItemSortOrderChanged(string sortOrder)
        {
            if (sortOrder == "asc")
                SortOrder = FilterSortOrder.Ascending;
            else
                SortOrder = FilterSortOrder.Descending;
        }
        public void ItemSortTypeChanged(string sortType)
        {
            if (sortType == "title")
                SortType = FilterSortType.ByTitle;
            else
                SortType = FilterSortType.ByDate;
        }
        public void SortItems()
        {
            var sortOrder = (SortOrder == FilterSortOrder.Ascending ? ObservableCollectionExtensions.SortOrder.Ascending : ObservableCollectionExtensions.SortOrder.Descending);
            if (SortType == FilterSortType.ByTitle)
                Items.Sort(i => i.Model.Title, sortOrder);
            else
                Items.Sort(i => i.Model.CreationDate, sortOrder);
        }

        public async Task SearchQueryChangedAsync(AutoSuggestBoxTextChangedEventArgs args)
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                CurrentFilterProperties.SearchQuery = string.Empty;
                return;
            }

            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                CurrentFilterProperties.SearchQuery = SearchQuery;
                var searchItems = await _dataService.GetItemsAsync(CurrentFilterProperties);

                SearchSuggestions.Clear();
                foreach (var item in searchItems)
                    SearchSuggestions.Add(new SearchResult(item.Id, item.Title));
            }
        }
        public void SearchQuerySubmitted(AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
                NavigationService.Navigate(typeof(Views.SingleItemPage), (args.ChosenSuggestion as SearchResult).Id);
            else
            {
                CurrentFilterProperties.SearchQuery = args.QueryText;
            }
        }
        public void DomainQueryChanged(AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                CurrentFilterProperties.DomainName = DomainQuery;
                DomainNameSuggestions.Replace(DomainNames.Where(d => d.Contains(DomainQuery)).ToList());
            }
        }
        public void DomainQuerySubmitted(AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
                CurrentFilterProperties.DomainName = args.QueryText;
        }
        public void TagQueryChanged(AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
                TagSuggestions.Replace(Tags.Where(t => t.Label.Contains(TagQuery)).ToList());
        }
        public void TagQuerySubmitted(AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
                CurrentFilterProperties.FilterTag = args.ChosenSuggestion as Tag;
        }
        public void EstimatedReadingTimeFilterChanged(string readingTime)
        {
            if (readingTime == "short")
            {
                EstimatedReadingTime = FilterEstimatedReadingTime.Short;
                CurrentFilterProperties.MinimumEstimatedReadingTime = 0;
                CurrentFilterProperties.MaximumEstimatedReadingTime = 5;
            }
            else if (readingTime == "medium")
            {
                EstimatedReadingTime = FilterEstimatedReadingTime.Medium;
                CurrentFilterProperties.MinimumEstimatedReadingTime = 5;
                CurrentFilterProperties.MaximumEstimatedReadingTime = 15;
            }
            else
            {
                EstimatedReadingTime = FilterEstimatedReadingTime.Long;
                CurrentFilterProperties.MinimumEstimatedReadingTime = 15;
                CurrentFilterProperties.MaximumEstimatedReadingTime = 1337;
            }
        }
        public void CreationDateFilterChanged(CalendarDatePickerDateChangedEventArgs args)
        {
            CurrentFilterProperties.CreationDateFrom = MinDateNullable;
            CurrentFilterProperties.CreationDateTo = MaxDateNullable;
        }
    }
}