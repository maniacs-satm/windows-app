using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using wallabag.Common;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web.Http;
using System.Linq;

namespace wallabag.DataModel
{
    [ImplementPropertyChanged]
    public sealed class DataSource
    {
        private static DataSource _wallabagDataSource = new DataSource();

        public static ObservableCollection<ItemViewModel> Items { get; set; }

        public static async Task<bool> GetItemsAsync(int page = 1)
        {
            HttpClient http = new HttpClient();

            await Helpers.AddHeaders(http, new User() { Username = "wallabag", Password = "wallabag" });
            System.Diagnostics.Debug.WriteLine(http.DefaultRequestHeaders["X-WSSE"]);
            var response = await http.GetAsync(new Uri(string.Format("http://v2.wallabag.org/api/entries.json?page={0}", page)));
            http.Dispose();

            if (response.StatusCode == HttpStatusCode.Ok ||
                response.StatusCode == HttpStatusCode.NoContent)
            {
                List<Item> items = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<List<Item>>(response.Content.ToString()));
                Items = new ObservableCollection<ItemViewModel>();
                foreach (Item i in items)
                    Items.Add(new ItemViewModel(i));
                return true;
            }
            return false;
        }
        public static async Task<ItemViewModel> GetItemAsync(int Id)
        {
            //await GetDataAsync(true);
            if (Items.Count > 0)
                return Items.Single(x => x.Model.Id == Id);

            return null;
        }

        private static async Task<bool> GetDataAsync(bool singleItem = false)
        {
            if (singleItem || !Helpers.IsConnectedToInternet())
                return await RestoreItemsAsync();
            return await GetItemsAsync();
        }

        private static async Task<bool> SaveItemsAsync()
        {
            try
            {
                //MemoryStream sessionData = new MemoryStream();
                //DataContractSerializer serializer = new DataContractSerializer(typeof(ObservableDictionary), new List<Type>() { typeof(Item) });
                //serializer.WriteObject(sessionData, Items);

                //StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync("items.xml", CreationCollisionOption.ReplaceExisting);
                //using (Stream fileStream = await file.OpenStreamForWriteAsync())
                //{
                //    sessionData.Seek(0, SeekOrigin.Begin);
                //    await sessionData.CopyToAsync(fileStream);
                //}
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
        private static async Task<bool> RestoreItemsAsync()
        {
            try
            {
                //var _temp = new Dictionary<string, object>();
                //StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("items.xml");

                //using (IInputStream inStream = await file.OpenSequentialReadAsync())
                //{
                //    DataContractSerializer serializer = new DataContractSerializer(typeof(Dictionary<string, object>), new List<Type>() { typeof(Item) });
                //    _temp = (Dictionary<string, object>)serializer.ReadObject(inStream.AsStreamForRead());
                //}
                //Items.Clear();
                //foreach (var i in _temp)
                //    Items.Add(i.Key, i.Value);
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
