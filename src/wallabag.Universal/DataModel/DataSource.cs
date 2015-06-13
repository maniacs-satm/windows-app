using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PropertyChanged;
using SQLite;
using wallabag.Common;
using Windows.Web.Http;

namespace wallabag.DataModel
{
    [ImplementPropertyChanged]
    public sealed class DataSource
    {
        private static string DATABASE_PATH { get; } = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "wallabag.db");

        public enum ItemType
        {
            Unread,
            Favorites,
            Archived,
            Deleted
        }

        public static async Task<List<Item>> GetItemsAsync(ItemType itemType)
        {
            SQLiteAsyncConnection conn = new SQLiteAsyncConnection(DATABASE_PATH);

            switch (itemType)
            {
                case ItemType.Unread:
                    return await conn.Table<Item>().Where(i => i.IsArchived == false && i.IsDeleted == false && i.IsStarred == false).ToListAsync();
                case ItemType.Favorites:
                    return await conn.Table<Item>().Where(i => i.IsDeleted == false && i.IsStarred == true).ToListAsync();
                case ItemType.Archived:
                    return await conn.Table<Item>().Where(i => i.IsArchived == true && i.IsDeleted == false).ToListAsync();
                case ItemType.Deleted:
                    return await conn.Table<Item>().Where(i => i.IsDeleted == true).ToListAsync();
            }
            return new List<Item>();
        }
        public static async Task<Item> GetItemAsync(int Id)
        {
            SQLiteAsyncConnection conn = new SQLiteAsyncConnection(DATABASE_PATH);
            return await conn.GetAsync<Item>(i => i.Id == Id);
        }

        public static async Task<bool> RefreshItems()
        {
            HttpClient http = new HttpClient();
            await Helpers.AddHeaders(http);

            try
            {
                var response = await http.GetAsync(new Uri($"{AppSettings.Instance.wallabagUrl}/api/entries.json"));
                http.Dispose();

                if (response.StatusCode == HttpStatusCode.Ok)
                {
                    var json = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<RootObject>(response.Content.ToString()));
                    SQLiteAsyncConnection conn = new SQLiteAsyncConnection(DATABASE_PATH);

                    foreach (var item in json.Embedded.Items)
                    {
                        var result = await (conn.Table<Item>().Where(i => i.Id == item.Id)).FirstOrDefaultAsync();

                        if (result == null)
                            await conn.InsertAsync(item);
                        else
                        {
                            result.Title = item.Title;
                            result.Url = item.Url;
                            result.IsArchived = item.IsArchived;
                            result.IsStarred = item.IsStarred;
                            result.IsDeleted = item.IsDeleted;
                            result.Content = item.Content;
                            result.CreatedAt = item.CreatedAt;
                            result.UpdatedAt = item.UpdatedAt;
                            result.TagsString = item.TagsString;
                           
                            await conn.UpdateAsync(result);
                        }
                    }
                    return true;
                }
                else
                    return false;
            }
            catch { return false; }
        }
        public static async Task<bool> AddItem(string url, string tags = "", string title = "")
        {
            HttpClient http = new HttpClient();

            await Helpers.AddHeaders(http);

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("url", url);
            parameters.Add("tags", tags);
            parameters.Add("title", title);

            var content = new HttpStringContent(JsonConvert.SerializeObject(parameters), Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json");

            try
            {
                var response = await http.PostAsync(new Uri($"{AppSettings.Instance.wallabagUrl}/api/entries.json"), content);
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

        public static async Task InitializeDatabase()
        {
            // TODO: Currently it's replacing the database every time. In the final release it should stop doing it.
            await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync("wallabag.db", Windows.Storage.CreationCollisionOption.ReplaceExisting);
            SQLiteAsyncConnection conn = new SQLiteAsyncConnection(DATABASE_PATH);
            await conn.CreateTableAsync<Item>();
        }
    }
}
