using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using wallabag.Common;
using Windows.Storage;
using Windows.Web.Http;

namespace wallabag.DataModel
{
    [ImplementPropertyChanged]
    public sealed class DataSource
    {
        private static DataSource _wallabagDataSource = new DataSource();
        public static ObservableDictionary Items { get; set; }

        public static async Task<bool> GetItemsAsync(int page = 1, bool IsSingleItem = false)
        {
            if (!Helpers.IsConnectedToInternet() || IsSingleItem)
                await RestoreItemsAsync();
            else
            {
                HttpClient http = new HttpClient();

                await Helpers.AddHeaders(http);
                System.Diagnostics.Debug.WriteLine(http.DefaultRequestHeaders["X-WSSE"]);
                var response = await http.GetAsync(new Uri(string.Format("http://v2.wallabag.org/api/entries.json?page={0}", page)));
                http.Dispose();

                if (response.StatusCode == HttpStatusCode.Ok ||
                    response.StatusCode == HttpStatusCode.NoContent)
                {
                    List<Item> items = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<List<Item>>(response.Content.ToString()));
                    Items = new ObservableDictionary();
                    foreach (Item i in items)
                        Items.Add(i.Id.ToString(), new ItemViewModel(i));
                    await SaveItemsAsync();
                    return true;
                }
            }
            return false;
        }
        public static async Task<ItemViewModel> GetItemAsync(int Id)
        {
            if (Items.Count == 0)
                await GetItemsAsync(0, true);
            if (Items.Count > 0 && Items.ContainsKey(Id.ToString()))
                return (ItemViewModel)Items[Id.ToString()];
            return null;
        }
        public static async Task<bool> AddItem(string url, string tags)
        {
            HttpClient http = new HttpClient();

            await Helpers.AddHeaders(http);

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("url", url);
            parameters.Add("tags", tags);

            var content = new HttpStringContent(JsonConvert.SerializeObject(parameters));
            var response = await http.PostAsync(new Uri("http://v2.wallabag.org/api/entries.json"), content);
            http.Dispose();

            if (response.StatusCode == HttpStatusCode.Ok)
                return true;
            return false;
        }

        public static async Task<bool> SaveItemsAsync()
        {
            try
            {
                string json = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(Items));

                StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync("items.json", CreationCollisionOption.ReplaceExisting);
                await Windows.Storage.FileIO.WriteTextAsync(file, json);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static async Task<bool> RestoreItemsAsync()
        {
            try
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("items.json");
                string json = await Windows.Storage.FileIO.ReadTextAsync(file);

                Items = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<ObservableDictionary>(json));

                return true;
            }
            catch (FileNotFoundException)
            {
                System.Diagnostics.Debug.WriteLine("File not found.");
                return false;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                return false;
            }
        }
    }
}
