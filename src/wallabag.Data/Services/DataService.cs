using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PropertyChanged;
using SQLite;
using wallabag.Common;
using wallabag.Models;
using wallabag.ViewModels;
using Windows.Web.Http;

namespace wallabag.Services
{
    [ImplementPropertyChanged]
    public sealed class DataService
    {
        private static SQLiteAsyncConnection conn = new SQLiteAsyncConnection(Helpers.DATABASE_PATH);
        private static int _lastItemId = 0;

        public static DateTime LastUserSyncDateTime
        {
            get { return DateTime.Parse(Windows.Storage.ApplicationData.Current.LocalSettings.Values["LastUserSyncDateTime"] as string ?? DateTime.Now.ToString()); }
            set { Windows.Storage.ApplicationData.Current.LocalSettings.Values["LastUserSyncDateTime"] = value.ToString(); }
        }

        public static async Task InitializeDatabaseAsync()
        {
            await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync(Helpers.DATABASE_FILENAME, Windows.Storage.CreationCollisionOption.OpenIfExists);
            SQLiteAsyncConnection conn = new SQLiteAsyncConnection(Helpers.DATABASE_PATH);
            await conn.CreateTableAsync<Item>();
            await conn.CreateTableAsync<Tag>();
            await conn.CreateTableAsync<OfflineTask>();
        }

        public static async Task<bool> SyncWithServerAsync()
        {
            if (!Helpers.IsConnectedToTheInternet)
                return false;

            var tasks = await conn.Table<OfflineTask>().ToListAsync();

            bool success = false;
            foreach (var task in tasks)
            {
                switch (task.Task)
                {
                    case OfflineTask.OfflineTaskType.AddItem:
                        success = await AddItemAsync(task.Url, task.TagsString, string.Empty, true);
                        break;
                    case OfflineTask.OfflineTaskType.DeleteItem:
                        success = await ItemViewModel.DeleteItemAsync(task.ItemId, true);
                        break;
                    case OfflineTask.OfflineTaskType.AddTags:
                        success = (await ItemViewModel.AddTagsAsync(task.ItemId, task.TagsString, true)) != null;
                        break;
                    case OfflineTask.OfflineTaskType.DeleteTag:
                        success = await ItemViewModel.DeleteTagAsync(task.ItemId, task.TagId, true);
                        break;
                    case OfflineTask.OfflineTaskType.MarkItemAsRead:
                        success = await ItemViewModel.UpdateSpecificProperty(task.ItemId, "archive", true);
                        break;
                    case OfflineTask.OfflineTaskType.UnmarkItemAsRead:
                        success = await ItemViewModel.UpdateSpecificProperty(task.ItemId, "archive", false);
                        break;
                    case OfflineTask.OfflineTaskType.MarkItemAsFavorite:
                        success = await ItemViewModel.UpdateSpecificProperty(task.ItemId, "star", true);
                        break;
                    case OfflineTask.OfflineTaskType.UnmarkItemAsFavorite:
                        success = await ItemViewModel.UpdateSpecificProperty(task.ItemId, "star", true);
                        break;
                }
                if (success)
                    await conn.DeleteAsync(task);
            }
            if (success)
                return true;
            else
                return false;
        }
        public static async Task<bool> DownloadItemsFromServerAsync()
        {
            var response = await Helpers.ExecuteHttpRequestAsync(Helpers.HttpRequestMethod.Get, "/entries");

            if (response.StatusCode == HttpStatusCode.Ok)
            {
                var json = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<RootObject>(response.Content.ToString()));

                // Regular expression to remove multiple whitespaces (including newline etc.)
                Regex Regex = new Regex("\\s+");

                foreach (var item in json.Embedded.Items)
                {
                    var existingItem = await (conn.Table<Item>().Where(i => i.Id == item.Id)).FirstOrDefaultAsync();

                    if (existingItem == null)
                    {
                        item.Title = Regex.Replace(item.Title, " ");

                        // If the title starts with a space, remove it.
                        if (item.Title.StartsWith(" "))
                            item.Title = item.Title.Remove(0, 1);
                        await conn.InsertAsync(item);
                    }
                    else
                    {
                        existingItem.Title = Regex.Replace(item.Title, " ");
                        existingItem.Url = item.Url;
                        existingItem.IsRead = item.IsRead;
                        existingItem.IsStarred = item.IsStarred;
                        existingItem.IsDeleted = item.IsDeleted;
                        existingItem.Content = item.Content;
                        existingItem.CreationDate = item.CreationDate;
                        existingItem.LastUpdated = item.LastUpdated;
                        existingItem.DomainName = item.DomainName;
                        existingItem.EstimatedReadingTime = item.EstimatedReadingTime;
                        existingItem.PreviewPictureUri = item.PreviewPictureUri;

                        await conn.UpdateAsync(existingItem);
                    }

                    foreach (Tag tag in item.Tags)
                    {
                        var existingTag = await (conn.Table<Tag>().Where(i => i.Id == tag.Id)).FirstOrDefaultAsync();
                        if (existingTag == null)
                            await conn.InsertAsync(tag);
                    }
                }
                return true;
            }
            else
                return false;

        }

        public static async Task<List<Item>> GetItemsAsync(FilterProperties filterProperties)
        {
            List<Item> result = new List<Item>();
            var allItems = await conn.Table<Item>().ToListAsync();
            if (allItems.Count > 0)
                _lastItemId = allItems.Last().Id;

            switch (filterProperties.ItemType)
            {
                case FilterProperties.FilterPropertiesItemType.All:
                    result = allItems;
                    break;
                case FilterProperties.FilterPropertiesItemType.Unread:
                    result = await conn.Table<Item>().Where(i => i.IsRead == false && i.IsDeleted == false).ToListAsync();
                    break;
                case FilterProperties.FilterPropertiesItemType.Favorites:
                    result = await conn.Table<Item>().Where(i => i.IsDeleted == false && i.IsStarred == true).ToListAsync();
                    break;
                case FilterProperties.FilterPropertiesItemType.Archived:
                    result = await conn.Table<Item>().Where(i => i.IsRead == true && i.IsDeleted == false).ToListAsync();
                    break;
                case FilterProperties.FilterPropertiesItemType.Deleted:
                    result = await conn.Table<Item>().Where(i => i.IsDeleted == true).ToListAsync();
                    break;
            }

            if (filterProperties.FilterTag != null)
                result = new List<Item>(result.Where(i => i.Tags.ToCommaSeparatedString().Contains(filterProperties.FilterTag.Label)));
            if (!string.IsNullOrEmpty(filterProperties.DomainName))
                result = new List<Item>(result.Where(i => i.DomainName.Equals(filterProperties.DomainName)));
            if (filterProperties.MinimumEstimatedReadingTime != null &&
                filterProperties.MaximumEstimatedReadingTime != null)
                result = new List<Item>(result.Where(i => i.EstimatedReadingTime >= filterProperties.MinimumEstimatedReadingTime
                                                    && i.EstimatedReadingTime <= filterProperties.MaximumEstimatedReadingTime));
            if (filterProperties.CreationDateFrom != null &&
                filterProperties.CreationDateTo != null)
            {
                result = new List<Item>(result.Where(i => i.CreationDate < filterProperties.CreationDateTo
                                        && i.CreationDate > filterProperties.CreationDateFrom));
            }

            if (filterProperties.SortOrder == FilterProperties.FilterPropertiesSortOrder.Ascending)
                result = new List<Item>(result.OrderBy(i => i.CreationDate));
            else
                result = new List<Item>(result.OrderByDescending(i => i.CreationDate));

            return result;
        }
        public static async Task<List<Tag>> GetTagsAsync()
        {
            List<Tag> result = new List<Tag>();
            return new List<Tag>((await conn.Table<Tag>().ToListAsync()).OrderBy(i => i.Label));
        }

        public static async Task<Item> GetItemAsync(int Id)
        {
            return await conn.GetAsync<Item>(i => i.Id == Id);
        }
        public static async Task<Item> GetItemAsync(string Title)
        {
            return await conn.GetAsync<Item>(i => i.Title == Title);
        }

        public static async Task<bool> AddItemAsync(string Url, string TagsString = "", string Title = "", bool IsOfflineAction = false)
        {
            if (!IsOfflineAction)
            {
                var newItem = new Item();
                var hostName = new Uri(Url).Host;
                newItem.Id = _lastItemId + 1;
                newItem.Title = hostName;
                newItem.DomainName = hostName;
                newItem.Tags = TagsString.ToObservableCollection();
                newItem.Url = Url;

                await conn.InsertAsync(newItem);
            }

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("url", Url);
            parameters.Add("tags", TagsString);
            parameters.Add("title", Title);

            var response = await Helpers.ExecuteHttpRequestAsync(Helpers.HttpRequestMethod.Post, "/entries", parameters);
            if (response.StatusCode == HttpStatusCode.Ok)
                return true;
            else
            {
                if (!IsOfflineAction)
                    await conn.InsertAsync(new OfflineTask()
                    {
                        Task = OfflineTask.OfflineTaskType.AddItem,
                        Url = Url,
                        TagsString = TagsString
                    });
                return false;
            }
        }
    }

    [ImplementPropertyChanged]
    public class FilterProperties
    {
        public FilterPropertiesItemType ItemType { get; set; } = FilterPropertiesItemType.Unread;
        public FilterPropertiesSortOrder SortOrder { get; set; } = FilterPropertiesSortOrder.Descending;
        public Tag FilterTag { get; set; }
        public string DomainName { get; set; }
        public int? MinimumEstimatedReadingTime { get; set; }
        public int? MaximumEstimatedReadingTime { get; set; }
        public DateTimeOffset? CreationDateFrom { get; set; }
        public DateTimeOffset? CreationDateTo { get; set; }

        public enum FilterPropertiesSortOrder { Ascending, Descending }
        public enum FilterPropertiesItemType { All, Unread, Favorites, Archived, Deleted }
    }
}
