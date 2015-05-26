using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
        public static ObservableCollection<ItemViewModel> Items { get; set; }

        public static async Task<bool> GetItemsAsync()
        {
            await RestoreItemsAsync();

            HttpClient http = new HttpClient();

            await Helpers.AddHeaders(http);
            var response = await http.GetAsync(new Uri($"{AppSettings.Instance.wallabagUrl}/api/entries.json"));
            http.Dispose();

            if (response.StatusCode == HttpStatusCode.Ok)
            {
                var json = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<RootObject>(response.Content.ToString()));
                Items = new ObservableCollection<ItemViewModel>();
                foreach (var item in json.Embedded.Items)
                {
                    Items.Add(new ItemViewModel(item));
                }
                await SaveItemsAsync();
                return true;
            }
            else if (response.StatusCode == HttpStatusCode.NoContent)
                return true;
            else
                return false;
        }
        public static async Task<ItemViewModel> GetItemAsync(int Id)
        {
            if (Items.Count == 0)
                await GetItemsAsync();
            if (Items.Count > 0)
                return Items.Where(i => i.Model.Id == Id).First();
            return null;
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
            var response = await http.PostAsync(new Uri($"{AppSettings.Instance.wallabagUrl}/api/entries.json"), content);
            http.Dispose();

            if (response.StatusCode == HttpStatusCode.Ok)
            {
                Item result = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<Item>(response.Content.ToString()));
                if (Items != null)
                    Items.Add(new ItemViewModel(result));
                return true;
            }
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

                Items = new ObservableCollection<ItemViewModel>();
                Items = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<ObservableCollection<ItemViewModel>>(json));

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
