using Newtonsoft.Json;
using PropertyChanged;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using wallabag.Common;
using wallabag.Data.Interfaces;
using wallabag.Data.Models;
using wallabag.Models;
using Windows.Foundation;
using Windows.Web.Http;
using static wallabag.Common.Helpers;

namespace wallabag.Data.Services
{
    [ImplementPropertyChanged]
    public sealed class RuntimeDataService : IDataService
    {
        private SQLiteAsyncConnection conn = new SQLiteAsyncConnection(DATABASE_PATH);
        private int _lastItemId = 0;

        public bool CredentialsAreExisting { get; } = !string.IsNullOrEmpty(AppSettings.AccessToken) && !string.IsNullOrEmpty(AppSettings.RefreshToken) && !string.IsNullOrEmpty(AppSettings.wallabagUrl);

        public async Task InitializeDatabaseAsync()
        {
            await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync(DATABASE_FILENAME, Windows.Storage.CreationCollisionOption.OpenIfExists);
            SQLiteAsyncConnection conn = new SQLiteAsyncConnection(DATABASE_PATH);
            await conn.CreateTableAsync<Item>();
            await conn.CreateTableAsync<Tag>();
            await conn.CreateTableAsync<OfflineTask>();
        }

        public async Task<bool> SyncOfflineTasksWithServerAsync()
        {
            if (!IsConnectedToTheInternet)
                return false;

            var tasks = await conn.Table<OfflineTask>().ToListAsync();

            bool success = false;
            foreach (var task in tasks)
                success = await task.ExecuteAsync();
            return success;
        }
        public IAsyncOperationWithProgress<bool, DownloadProgress> DownloadItemsFromServerAsync(bool DownloadAllItems = false)
        {
            Func<CancellationToken, IProgress<DownloadProgress>, Task<bool>> taskProvider = (token, progress) => _DownloadItemsFromServerAsync(progress, DownloadAllItems);
            return AsyncInfo.Run(taskProvider);
        }
        private async Task<bool> _DownloadItemsFromServerAsync(IProgress<DownloadProgress> progress, bool DownloadAllItems)
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
                        Dictionary<string, object> parameters = new Dictionary<string, object>() { ["page"] = i };
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

                    item.Title = Regex.Replace(item.Title, " ");

                    // If the title starts with a space, remove it.
                    if (item.Title.StartsWith(" "))
                        item.Title = item.Title.Remove(0, 1);

                    if (item.PreviewPictureUri.StartsWith("//"))
                        item.PreviewPictureUri = $"https:{item.PreviewPictureUri}";

                    if (existingItem == null)
                    {
                        await conn.InsertAsync(item);
                    }
                    else
                    {
                        existingItem = item;
                        await existingItem.UpdateAsync();
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

        public async Task<List<Item>> GetItemsAsync(FilterProperties filterProperties)
        {
            string sqlQuery = "SELECT * FROM 'Items' ";
            List<object> sqlParams = new List<object>();
            List<Item> result = new List<Item>();

            var allItems = await conn.Table<Item>().ToListAsync();
            string queryConnector = "AND";

            if (allItems.Count > 0)
                _lastItemId = allItems.Last().Id;

            switch (filterProperties.ItemType)
            {
                case FilterProperties.FilterPropertiesItemType.All:
                    queryConnector = "WHERE";
                    break;
                case FilterProperties.FilterPropertiesItemType.Unread:
                    sqlQuery += "WHERE IsRead = ? ";
                    sqlParams.Add(0); // 0 == false, 1 == true
                    break;
                case FilterProperties.FilterPropertiesItemType.Favorites:
                    sqlQuery += "WHERE IsStarred = ? ";
                    sqlParams.Add(1);
                    break;
                case FilterProperties.FilterPropertiesItemType.Archived:
                    sqlQuery += "WHERE IsRead = ? ";
                    sqlParams.Add(1);
                    break;
            }

            if (filterProperties.FilterTag != null)
            {
                sqlQuery += $"{queryConnector} Tags LIKE ?";
                sqlParams.Add(filterProperties.FilterTag.Label);
                queryConnector = "AND";
            }
            if (!string.IsNullOrEmpty(filterProperties.DomainName))
            {
                sqlQuery += $"{queryConnector} DomainName = ?";
                sqlParams.Add(filterProperties.DomainName);
                queryConnector = "AND";
            }
            if (!string.IsNullOrWhiteSpace(filterProperties.SearchQuery))
            {
                sqlQuery += $"{queryConnector} Title LIKE ?";
                sqlParams.Add($"%{filterProperties.SearchQuery}%");
                queryConnector = "AND";
            }

            result = await conn.QueryAsync<Item>(sqlQuery, sqlParams.ToArray());

            if (result != null)
            {
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
            }
            return result;
        }
        public async Task<List<Tag>> GetTagsAsync()
        {
            List<Tag> result = await conn.Table<Tag>().ToListAsync();
            if (result != null)
                return new List<Tag>(result.OrderBy(i => i.Label));
            return result;
        }

        public async Task<Item> GetItemAsync(int Id)
        {
            return await conn.GetAsync<Item>(i => i.Id == Id);
        }
        public async Task<Item> GetItemAsync(string Title)
        {
            return await conn.GetAsync<Item>(i => i.Title == Title);
        }

        public async Task<bool> AddItemAsync(string Url, string TagsString = "", string Title = "", bool IsOfflineTask = false)
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

                if (existingItem == null)
                    await conn.InsertAsync(item);
                else
                {
                    existingItem = item;
                    await existingItem.UpdateAsync();
                }

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

                    _lastItemId += 1;
                    await conn.InsertAsync(newItem);
                    await OfflineTask.AddTaskAsync(newItem, OfflineTask.OfflineTaskAction.AddItem, "/entries", parameters, HttpRequestMethod.Post);
                }
                return false;
            }
        }

        public Task UpdateItemAsync(Item item) => conn.UpdateAsync(item);
        public Task<List<OfflineTask>> GetOfflineTasksAsync() => conn.Table<OfflineTask>().ToListAsync();
        public Task<bool> LoginAsync(string url, string username, string password) => AuthorizationService.RequestTokenAsync(username, password, url);
        public Task DeleteItemAsync(Item item) => conn.DeleteAsync(item);
    }
}
