using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PropertyChanged;
using SQLite;
using wallabag.Common;
using wallabag.Models;
using Windows.Web.Http;

namespace wallabag.Services
{
    [ImplementPropertyChanged]
    public sealed class DataSource
    {
        private static string DATABASE_PATH { get; } = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "wallabag.db");

        public static async Task<List<Item>> GetItemsAsync(FilterProperties filterProperties)
        {
            SQLiteAsyncConnection conn = new SQLiteAsyncConnection(DATABASE_PATH);
            List<Item> result = new List<Item>();

            switch (filterProperties.itemType)
            {
                case FilterProperties.ItemType.All:
                    result = await conn.Table<Item>().ToListAsync();
                    break;
                case FilterProperties.ItemType.Unread:
                    result = await conn.Table<Item>().Where(i => i.IsRead == false && i.IsDeleted == false && i.IsStarred == false).ToListAsync();
                    break;
                case FilterProperties.ItemType.Favorites:
                    result = await conn.Table<Item>().Where(i => i.IsDeleted == false && i.IsStarred == true).ToListAsync();
                    break;
                case FilterProperties.ItemType.Archived:
                    result = await conn.Table<Item>().Where(i => i.IsRead == true && i.IsDeleted == false).ToListAsync();
                    break;
                case FilterProperties.ItemType.Deleted:
                    result = await conn.Table<Item>().Where(i => i.IsDeleted == true).ToListAsync();
                    break;
            }

            if (filterProperties.FilterTag != null)
                result = new List<Item>(result.Where(i => i.Tags.ToCommaSeparatedString().Contains(filterProperties.FilterTag.Label)));

            if (filterProperties.sortOrder == FilterProperties.SortOrder.Ascending)
                result = new List<Item>(result.OrderBy(i => i.CreationDate));
            else
                result = new List<Item>(result.OrderByDescending(i => i.CreationDate));

            return result;
        }
        public static async Task<Item> GetItemAsync(int Id)
        {
            SQLiteAsyncConnection conn = new SQLiteAsyncConnection(DATABASE_PATH);
            return await conn.GetAsync<Item>(i => i.Id == Id);
        }
        public static async Task<Item> GetItemAsync(string Title)
        {
            SQLiteAsyncConnection conn = new SQLiteAsyncConnection(DATABASE_PATH);
            return await conn.GetAsync<Item>(i => i.Title == Title);
        }

        public static async Task<List<Tag>> GetTagsAsync()
        {
            SQLiteAsyncConnection conn = new SQLiteAsyncConnection(DATABASE_PATH);
            List<Tag> result = new List<Tag>();

            result = new List<Tag>((await conn.Table<Tag>().ToListAsync()).OrderBy(i => i.Label));

            int colorIndex = 0;
            foreach (Tag tag in result)
            {
                colorIndex += 1;

                if (colorIndex / 17 == 1)
                    colorIndex = 0;

                tag.Color = Tag.PossibleColors[colorIndex];
            }

            return result;
        }

        public static async Task<bool> DownloadItemsFromServerAsync()
        {
            HttpClient http = new HttpClient();
            await Helpers.AddHttpHeadersAsync(http);

            try
            {
                var response = await http.GetAsync(new Uri($"{AppSettings.wallabagUrl}/api/entries.json"));
                http.Dispose();

                if (response.StatusCode == HttpStatusCode.Ok)
                {
                    var json = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<RootObject>(response.Content.ToString()));
                    SQLiteAsyncConnection conn = new SQLiteAsyncConnection(DATABASE_PATH);

                    foreach (var item in json.Embedded.Items)
                    {
                        var existingItem = await (conn.Table<Item>().Where(i => i.Id == item.Id)).FirstOrDefaultAsync();

                        if (existingItem == null)
                            await conn.InsertAsync(item);
                        else
                        {
                            existingItem.Title = item.Title;
                            existingItem.Url = item.Url;
                            existingItem.IsRead = item.IsRead;
                            existingItem.IsStarred = item.IsStarred;
                            existingItem.IsDeleted = item.IsDeleted;
                            existingItem.Content = item.Content;
                            existingItem.CreationDate = item.CreationDate;
                            existingItem.LastUpdated = item.LastUpdated;

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
            catch { return false; }
        }
        public static async Task<bool> AddItemAsync(string url, string tags = "", string title = "")
        {
            HttpClient http = new HttpClient();

            await Helpers.AddHttpHeadersAsync(http);

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("url", url);
            parameters.Add("tags", tags);
            parameters.Add("title", title);

            var content = new HttpStringContent(JsonConvert.SerializeObject(parameters), Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json");

            try
            {
                var response = await http.PostAsync(new Uri($"{AppSettings.wallabagUrl}/api/entries.json"), content);
                http.Dispose();

                if (response.StatusCode == HttpStatusCode.Ok)
                {
                    Item result = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<Item>(response.Content.ToString()));

                    SQLiteAsyncConnection conn = new SQLiteAsyncConnection(DATABASE_PATH);
                    await conn.InsertAsync(result);
                    return true;
                }
                return false;
            }
            catch { return false; }
        }

        public static async Task InitializeDatabaseAsync()
        {
            await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync("wallabag.db", Windows.Storage.CreationCollisionOption.OpenIfExists);
            SQLiteAsyncConnection conn = new SQLiteAsyncConnection(DATABASE_PATH);
            await conn.CreateTableAsync<Item>();
            await conn.CreateTableAsync<Tag>();
        }
    }

    [ImplementPropertyChanged]
    public class FilterProperties
    {
        public ItemType itemType { get; set; } = ItemType.Unread;
        public SortOrder sortOrder { get; set; } = SortOrder.Descending;
        public Tag FilterTag { get; set; }

        public enum SortOrder { Ascending, Descending }
        public enum ItemType { All, Unread, Favorites, Archived, Deleted }
    }
}
