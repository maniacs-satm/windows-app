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

        public bool OfflineTasksCountGreaterThanZero { get; set; } = false;

        public ObservableCollection<SearchResult> SearchSuggestions { get; set; } = new ObservableCollection<SearchResult>();
        public ObservableCollection<string> DomainNameSuggestions { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<Tag> TagSuggestions { get; set; } = new ObservableCollection<Tag>();

        public FilterProperties CurrentFilterProperties { get; set; } = new FilterProperties();
        public bool IsSyncing { get; set; } = false;

        public DelegateCommand SyncCommand { get; private set; }
        public DelegateCommand AddItemCommand { get; private set; }
        public DelegateCommand NavigateToSettingsPageCommand { get; private set; }
        public DelegateCommand<ItemClickEventArgs> ItemClickCommand { get; private set; }

        // Multiple selection
        public bool? IsMultipleSelectionEnabled { get; set; } = false;
        public DelegateCommand<SelectionChangedEventArgs> ItemSelectionChangedCommand { get; private set; }
        public ObservableCollection<ItemViewModel> SelectedItems { get; set; } = new ObservableCollection<ItemViewModel>();
        public DelegateCommand MarkItemsAsReadCommand { get; private set; }
        public DelegateCommand MarkItemsAsFavoriteCommand { get; private set; }
        public DelegateCommand UnmarkItemsAsReadCommand { get; private set; }
        public DelegateCommand UnmarkItemsAsFavoriteCommand { get; private set; }
        public DelegateCommand EditTagsCommand { get; private set; }
        public DelegateCommand DeleteItemsCommand { get; private set; }

        // Search
        public string HeaderText { get; set; } = Helpers.LocalizedString("ItemsHeaderTextBlock/Text");
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
        public DelegateCommand<SelectionChangedEventArgs> ItemTypeSelectionChangedCommand { get; set; }
        public int ItemTypeSelectedIndex { get; set; } = 1;
        public DelegateCommand<string> ItemSortOrderChangedCommand { get; set; }
        public DelegateCommand<string> ItemSortTypeChangedCommand { get; set; }
        public string DomainQuery { get; set; }
        public DelegateCommand<AutoSuggestBoxTextChangedEventArgs> DomainQueryChangedCommand { get; private set; }
        public DelegateCommand<AutoSuggestBoxQuerySubmittedEventArgs> DomainQuerySubmittedCommand { get; private set; }
        public string TagQuery { get; set; }
        public DelegateCommand<AutoSuggestBoxTextChangedEventArgs> TagQueryChangedCommand { get; private set; }
        public DelegateCommand<AutoSuggestBoxQuerySubmittedEventArgs> TagQuerySubmittedCommand { get; private set; }
        public DelegateCommand<string> EstimatedReadingTimeFilterChangedCommand { get; private set; }

        [AlsoNotifyFor("MinDate")]
        public DateTimeOffset? MinDateNullable { get; set; }
        public DateTimeOffset MinDate { get { return MinDateNullable ?? DateTimeOffset.Now; } }
        public DateTimeOffset? MaxDateNullable { get; set; }
        public DateTimeOffset MaxDate { get; } = DateTimeOffset.Now;
        public DelegateCommand<CalendarDatePickerDateChangedEventArgs> CreationDateFilterChangedCommand { get; private set; }
        public DelegateCommand ResetFilterCommand { get; private set; }
        public DelegateCommand FilterCommand { get; private set; }

        public MainViewModel(IDataService dataService)
        {
            _dataService = dataService;

            SyncCommand = new DelegateCommand(async () => await SyncWithServerAsync());
            NavigateToSettingsPageCommand = new DelegateCommand(() =>
            {
                NavigationService.Navigate(typeof(Views.SettingsPage), null, new DrillInNavigationTransitionInfo());
            });
            AddItemCommand = new DelegateCommand(async () => await Services.DialogService.ShowDialogAsync(Services.DialogService.Dialog.AddItem));

            ResetFilterCommand = new DelegateCommand(async () =>
            {
                CurrentFilterProperties = new FilterProperties();
                ItemTypeSelectedIndex = 1;
                SortType = FilterSortType.ByDate;
                SortOrder = FilterSortOrder.Descending;
                EstimatedReadingTime = FilterEstimatedReadingTime.Unfiltered;
                MinDateNullable = null;
                MaxDateNullable = null;
                await GetItemsFromDatabaseAsync();
            });
            FilterCommand = new DelegateCommand(async () =>
            {
                await GetItemsFromDatabaseAsync();
                SortItems();
            });
            ItemClickCommand = new DelegateCommand<ItemClickEventArgs>(args => ItemClick(args));

            ItemSelectionChangedCommand = new DelegateCommand<SelectionChangedEventArgs>(e =>
            {
                foreach (ItemViewModel item in e.AddedItems)
                    SelectedItems.Add(item);
                foreach (ItemViewModel item in e.RemovedItems)
                    SelectedItems.Remove(item);
            });
            MarkItemsAsReadCommand = new DelegateCommand(async () => await MarkItemsAsReadAsync());
            UnmarkItemsAsReadCommand = new DelegateCommand(async () => await UnmarkItemsAsReadAsync());
            MarkItemsAsFavoriteCommand = new DelegateCommand(async () => await MarkItemsAsFavoriteAsync());
            UnmarkItemsAsFavoriteCommand = new DelegateCommand(async () => await UnmarkItemsAsFavoriteAsync());
            EditTagsCommand = new DelegateCommand(async () => await EditTagsAsync());
            DeleteItemsCommand = new DelegateCommand(async () => await DeleteItemsAsync());

            ItemTypeSelectionChangedCommand = new DelegateCommand<SelectionChangedEventArgs>(itemType => ItemTypeSelectionChanged(itemType));
            ItemSortOrderChangedCommand = new DelegateCommand<string>(sortOrder => ItemSortOrderChanged(sortOrder));
            ItemSortTypeChangedCommand = new DelegateCommand<string>(sortType => ItemSortTypeChanged(sortType));
            SearchQueryChangedCommand = new DelegateCommand<AutoSuggestBoxTextChangedEventArgs>(async e => await SearchQueryChangedAsync(e));
            SearchQuerySubmittedCommand = new DelegateCommand<AutoSuggestBoxQuerySubmittedEventArgs>(async e => await SearchQuerySubmittedAsync(e));
            DomainQueryChangedCommand = new DelegateCommand<AutoSuggestBoxTextChangedEventArgs>(e => DomainQueryChanged(e));
            DomainQuerySubmittedCommand = new DelegateCommand<AutoSuggestBoxQuerySubmittedEventArgs>(e => DomainQuerySubmitted(e));
            TagQueryChangedCommand = new DelegateCommand<AutoSuggestBoxTextChangedEventArgs>(e => TagQueryChanged(e));
            TagQuerySubmittedCommand = new DelegateCommand<AutoSuggestBoxQuerySubmittedEventArgs>(e => TagQuerySubmitted(e));
            EstimatedReadingTimeFilterChangedCommand = new DelegateCommand<string>(readingTime => EstimatedReadingTimeFilterChanged(readingTime));
            CreationDateFilterChangedCommand = new DelegateCommand<CalendarDatePickerDateChangedEventArgs>(args => CreationDateFilterChanged(args));

            OfflineTasks.CollectionChanged += (s, e) =>
            {
                if (OfflineTasks.Count > 0)
                    OfflineTasksCountGreaterThanZero = true;
                else
                    OfflineTasksCountGreaterThanZero = false;
            };
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            await GetItemsFromDatabaseAsync();

            if (AppSettings.SyncOnStartup && mode != NavigationMode.Back)
                await SyncWithServerAsync();

            if (state.ContainsKey(nameof(CurrentFilterProperties)))
                CurrentFilterProperties = JsonConvert.DeserializeObject<FilterProperties>((string)state[nameof(CurrentFilterProperties)]);
            else
                CurrentFilterProperties = new FilterProperties();

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
                await SyncWithServerAsync();
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
                    await GetItemsFromDatabaseAsync();
                else if (message.Notification == "ResetSearch")
                {
                    CurrentFilterProperties.SearchQuery = string.Empty;
                    SearchQuery = string.Empty;
                    HeaderText = Helpers.LocalizedString("ItemsHeaderTextBlock/Text");
                    await GetItemsFromDatabaseAsync();
                }
                else if (message.Notification == "FilterView")
                    FilterCommand.Execute();
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

        public async Task SyncWithServerAsync()
        {
            IsSyncing = true;

            await _dataService.SyncOfflineTasksWithServerAsync();
            await _dataService.DownloadItemsFromServerAsync();
            await GetItemsFromDatabaseAsync();

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
        public async Task GetItemsFromDatabaseAsync()
        {
            bool sortDescending = SortOrder == FilterSortOrder.Descending;

            var itemsInDatabase = await _dataService.GetItemsAsync(CurrentFilterProperties);
            var currentItems = new List<Item>();

            foreach (var item in Items)
                currentItems.Add(item.Model);

            var dateTimeComparer = new ItemByDateTimeComparer();

            var newItems = itemsInDatabase.Except(currentItems).ToList();
            var changedItems = itemsInDatabase.Except(currentItems).Except(newItems).ToList();
            var removedItems = currentItems.Except(itemsInDatabase).ToList();

            foreach (var item in newItems)
                Items.AddSorted(new ItemViewModel(item, _dataService), dateTimeComparer, sortDescending);

            foreach (var item in changedItems)
            {
                Items.Remove(Items.Where(i => i.Model.Id == item.Id).First());
                Items.AddSorted(new ItemViewModel(item, _dataService), dateTimeComparer, sortDescending);

                await _dataService.UpdateItemAsync(item);
            }

            foreach (var item in removedItems)
                Items.Remove(new ItemViewModel(item, _dataService));

            Tags.Replace(await _dataService.GetTagsAsync());

            foreach (var item in Items)
                if (!DomainNames.Contains(item.Model.DomainName))
                    DomainNames.Add(item.Model.DomainName);

            DomainNames.Sort(d => d);

            await GetOfflineTasksAsync();
        }

        public async Task MarkItemsAsReadAsync()
        {
            foreach (var item in SelectedItems)
            {
                item.Model.IsRead = true;
                await OfflineTask.AddTaskAsync(item.Model, OfflineTask.OfflineTaskAction.MarkAsRead);
            }
            await FinishMultipleSelection();
        }
        public async Task UnmarkItemsAsReadAsync()
        {
            foreach (var item in SelectedItems)
            {
                item.Model.IsRead = false;
                await OfflineTask.AddTaskAsync(item.Model, OfflineTask.OfflineTaskAction.UnmarkAsRead);
            }
            await FinishMultipleSelection();
        }
        public async Task MarkItemsAsFavoriteAsync()
        {
            foreach (var item in SelectedItems)
            {
                item.Model.IsStarred = true;
                await OfflineTask.AddTaskAsync(item.Model, OfflineTask.OfflineTaskAction.MarkAsFavorite);
            }
            await FinishMultipleSelection();
        }
        public async Task UnmarkItemsAsFavoriteAsync()
        {
            foreach (var item in SelectedItems)
            {
                item.Model.IsStarred = false;
                await OfflineTask.AddTaskAsync(item.Model, OfflineTask.OfflineTaskAction.UnmarkAsFavorite);
            }
            await FinishMultipleSelection();
        }
        public async Task EditTagsAsync()
        {
            var newTags = new ObservableCollection<Tag>();
            var dialog = await Services.DialogService.ShowDialogAsync(Services.DialogService.Dialog.EditTags, newTags);

            if (dialog == ContentDialogResult.Primary && newTags.Count > 0)
                foreach (var item in SelectedItems)
                    await OfflineTask.AddTaskAsync(item.Model, OfflineTask.OfflineTaskAction.AddTags, newTags.ToCommaSeparatedString());
        }
        public async Task DeleteItemsAsync()
        {
            foreach (var item in SelectedItems)
                await item.DeleteAsync();

            await FinishMultipleSelection();
        }
        private async Task FinishMultipleSelection()
        {
            foreach (var item in SelectedItems)
                await _dataService.UpdateItemAsync(item.Model);

            IsItemClickEnabled = true;
            IsMultipleSelectionEnabled = false;

            Messenger.Default.Send(new NotificationMessage("FinishMultipleSelection"));

            await GetItemsFromDatabaseAsync();
            await _dataService.SyncOfflineTasksWithServerAsync();
        }

        public void ItemTypeSelectionChanged(SelectionChangedEventArgs e)
        {
            var allTranslated = Helpers.LocalizedString("AllItemTypeComboBoxItem/Content");
            var unreadTranslated = Helpers.LocalizedString("UnreadItemTypeComboBoxItem/Content");
            var favoritesTranslated = Helpers.LocalizedString("FavoritesItemTypeComboBoxItem/Content");
            var archivedTranslated = Helpers.LocalizedString("ArchivedItemTypeComboBoxItem/Content");

            var newItemType = (e.AddedItems.First() as ComboBoxItem).Content.ToString();
            if (newItemType.Equals(allTranslated))
                CurrentFilterProperties.ItemType = FilterProperties.FilterPropertiesItemType.All;
            else if (newItemType.Equals(unreadTranslated))
                CurrentFilterProperties.ItemType = FilterProperties.FilterPropertiesItemType.Unread;
            else if (newItemType.Equals(favoritesTranslated))
                CurrentFilterProperties.ItemType = FilterProperties.FilterPropertiesItemType.Favorites;
            else if (newItemType.Equals(archivedTranslated))
                CurrentFilterProperties.ItemType = FilterProperties.FilterPropertiesItemType.Archived;
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
                CurrentFilterProperties.ItemType = FilterProperties.FilterPropertiesItemType.Unread;
                return;
            }

            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                CurrentFilterProperties.SearchQuery = SearchQuery;
                CurrentFilterProperties.ItemType = FilterProperties.FilterPropertiesItemType.All;
                var searchItems = await _dataService.GetItemsAsync(CurrentFilterProperties);

                SearchSuggestions.Clear();
                foreach (var item in searchItems)
                    SearchSuggestions.Add(new SearchResult(item.Id, item.Title));

                SearchSuggestions.Sort(i => i.Id);
            }
        }
        public Task SearchQuerySubmittedAsync(AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                NavigationService.Navigate(typeof(Views.SingleItemPage), (args.ChosenSuggestion as SearchResult).Id);
                return Task.CompletedTask;
            }
            else
            {
                CurrentFilterProperties.SearchQuery = args.QueryText;
                HeaderText = $"\"{args.QueryText}\"".ToUpper();
                Messenger.Default.Send(new NotificationMessage("HideSearch"));
                return GetItemsFromDatabaseAsync();
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