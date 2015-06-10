using System;
using System.Collections.Generic;
using System.Linq;
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
        private const string DATABASE_PATH = "wallabag.db";

        public static async Task<List<Item>> GetItemsAsync()
        {
            await RestoreItemsAsync();

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

                    await conn.InsertAllAsync(json.Embedded.Items);
                    await SaveItemsAsync();
                    return json.Embedded.Items.ToList();
                }
                else
                    return new List<Item>();
            }
            catch { return new List<Item>(); }
        }
        public static async Task<Item> GetItemAsync(int Id)
        {
            SQLiteAsyncConnection conn = new SQLiteAsyncConnection(DATABASE_PATH);
            return await conn.GetAsync<Item>(i => i.Id == Id);
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
            SQLiteAsyncConnection conn = new SQLiteAsyncConnection(DATABASE_PATH);
            await conn.CreateTableAsync<Item>();
            await conn.CreateTableAsync<Tag>();
        }
    }
}
