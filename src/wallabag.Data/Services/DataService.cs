using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PropertyChanged;
using SQLite;
using wallabag.Common;
using wallabag.Models;
using Windows.Foundation;
using Windows.Web.Http;
using static wallabag.Common.Helpers;

namespace wallabag.Services
{
    [ImplementPropertyChanged]
    public sealed class DataService
    {
        private static SQLiteAsyncConnection conn = new SQLiteAsyncConnection(DATABASE_PATH);
        private static int _lastItemId = 0;

        public static DateTime LastUserSyncDateTime
        {
            get { return DateTime.Parse(Windows.Storage.ApplicationData.Current.LocalSettings.Values["LastUserSyncDateTime"] as string ?? DateTime.Now.ToString()); }
            set { Windows.Storage.ApplicationData.Current.LocalSettings.Values["LastUserSyncDateTime"] = value.ToString(); }
        }

        public static async Task InitializeDatabaseAsync()
        {
            await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync(DATABASE_FILENAME, Windows.Storage.CreationCollisionOption.OpenIfExists);
            SQLiteAsyncConnection conn = new SQLiteAsyncConnection(DATABASE_PATH);
            await conn.CreateTableAsync<Item>();
            await conn.CreateTableAsync<Tag>();
            await conn.CreateTableAsync<OfflineTask>();
        }

        public static async Task<bool> SyncOfflineTasksWithServerAsync()
        {
            if (!IsConnectedToTheInternet)
                return false;

            var tasks = await conn.Table<OfflineTask>().ToListAsync();

            bool success = false;
            foreach (var task in tasks)
                success = await task.ExecuteAsync();
            return success;
        }
        public static IAsyncOperationWithProgress<bool, DownloadProgress> DownloadItemsFromServerAsync(bool DownloadAllItems = false)
        {
            Func<CancellationToken, IProgress<DownloadProgress>, Task<bool>> taskProvider = (token, progress) => _DownloadItemsFromServerAsync(progress, DownloadAllItems);
            return AsyncInfo.Run(taskProvider);
        }
        private static async Task<bool> _DownloadItemsFromServerAsync(IProgress<DownloadProgress> progress, bool DownloadAllItems)
        {
            var dProgress = new DownloadProgress();
            var itemResponse = await ExecuteHttpRequestAsync(HttpRequestMethod.Get, "/entries");
            var tagResponse = await ExecuteHttpRequestAsync(HttpRequestMethod.Get, "/tags");

            if (itemResponse.StatusCode == HttpStatusCode.Ok &&
                tagResponse.StatusCode == HttpStatusCode.Ok)
            {
                #region itemResponse handling
                var itemJson = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<RootObject>(itemResponse.Content.ToString()));
                List<Item> downloadedItems = itemJson.Embedded.Items.ToList();

                dProgress.TotalNumberOfItems = itemJson.TotalNumberOfItems;
                progress.Report(dProgress);

                if (itemJson.Pages > 1 && DownloadAllItems)
                {
                    for (int i = 2; i <= itemJson.Pages; i++)
                    {
                        Dictionary<string, object> parameters = new Dictionary<string, object>() {["page"] = i };
                        var additionalHttpResponse = await ExecuteHttpRequestAsync(HttpRequestMethod.Get, "/entries", parameters);

                        var additionalJson = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<RootObject>(itemResponse.Content.ToString()));
                        foreach (var item in additionalJson.Embedded.Items)
                            if (!downloadedItems.Contains(item, new ItemComparer()))
                                downloadedItems.Add(item);
                    }

                }

                // Regular expression to remove multiple whitespaces (including newline etc.)
                Regex Regex = new Regex("\\s+");

                int index = 0;
                foreach (var item in downloadedItems)
                {
                    dProgress.CurrentItem = item;
                    dProgress.CurrentItemIndex = index;
                    progress.Report(dProgress);

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
                    index += 1;
                }
                #endregion

                var tagList = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<List<Tag>>(tagResponse.Content.ToString()));
                foreach (var item in tagList)
                {
                    var existingTag = await conn.Table<Tag>().Where(t => t.Label == item.Label).FirstOrDefaultAsync();
                    if (existingTag == null)
                        await conn.InsertAsync(item);
                }

                return true;
            }
            else
                return false;
        }

        public static async Task<List<Item>> GetItemsAsync(FilterProperties filterProperties)
        {
            string sqlQuery = "SELECT * FROM 'Items' ";
            List<object> sqlParams = new List<object>();
            List<Item> result = new List<Item>();

            var allItems = await conn.Table<Item>().ToListAsync();

            if (allItems.Count > 0)
                _lastItemId = allItems.Last().Id;

            switch (filterProperties.ItemType)
            {
                case FilterProperties.FilterPropertiesItemType.All: break;
                case FilterProperties.FilterPropertiesItemType.Unread:
                    sqlQuery += "WHERE IsRead = ? AND IsDeleted = ? ";
                    sqlParams.Add(0); // 0 == false, 1 == true
                    sqlParams.Add(0);
                    break;
                case FilterProperties.FilterPropertiesItemType.Favorites:
                    sqlQuery += "WHERE IsDeleted = ? AND IsStarred = ? ";
                    sqlParams.Add(0);
                    sqlParams.Add(1);
                    break;
                case FilterProperties.FilterPropertiesItemType.Archived:
                    sqlQuery += "WHERE IsRead = ? AND IsDeleted = ? ";
                    sqlParams.Add(1);
                    sqlParams.Add(0);
                    break;
                case FilterProperties.FilterPropertiesItemType.Deleted:
                    sqlQuery += "WHERE IsDeleted = ? ";
                    sqlParams.Add(1);
                    break;
            }

            if (filterProperties.FilterTag != null)
            {
                sqlQuery += "AND Tags LIKE ?";
                sqlParams.Add(filterProperties.FilterTag.Label);
            }
            if (!string.IsNullOrEmpty(filterProperties.DomainName))
            {
                sqlQuery += "AND DomainName = ?";
                sqlParams.Add(filterProperties.DomainName);
            }

            result = await conn.QueryAsync<Item>(sqlQuery, sqlParams.ToArray());

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

        public static async Task<bool> AddItemAsync(string Url, string TagsString = "", string Title = "", bool IsOfflineTask = false)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("url", Url);
            parameters.Add("tags", TagsString);
            parameters.Add("title", Title);

            var response = await ExecuteHttpRequestAsync(HttpRequestMethod.Post, "/entries", parameters);
            if (response.StatusCode == HttpStatusCode.Ok)
            {
                var item = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<Item>(response.Content.ToString()));
                var existingItem = await (conn.Table<Item>().Where(i => i.Id == item.Id)).FirstOrDefaultAsync();

                if (existingItem != null)
                {
                    existingItem.Id = item.Id;
                    existingItem.Title = item.Title;
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
                else
                    await conn.InsertAsync(item);

                foreach (Tag tag in item.Tags)
                {
                    var existingTag = await (conn.Table<Tag>().Where(i => i.Id == tag.Id)).FirstOrDefaultAsync();
                    if (existingTag == null)
                        await conn.InsertAsync(tag);
                }
                return true;
            }
            else
            {
                if (!IsOfflineTask)
                {
                    var newItem = new Item();
                    var hostName = new Uri(Url).Host;
                    newItem.Id = _lastItemId + 1;
                    newItem.Title = hostName;
                    newItem.DomainName = hostName;
                    newItem.Tags = TagsString.ToObservableCollection();
                    newItem.Url = Url;
                    newItem.CreationDate = DateTime.Now;

                    _lastItemId += 1;
                    await conn.InsertAsync(newItem);

                    await conn.InsertAsync(new OfflineTask("/entries", parameters, HttpRequestMethod.Post));
                }
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

    public class DownloadProgress
    {
        public int TotalNumberOfItems { get; set; }
        public int CurrentItemIndex { get; set; }
        public Item CurrentItem { get; set; }
    }
}
