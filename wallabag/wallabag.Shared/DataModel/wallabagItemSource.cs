using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using wallabag.Common;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web.Syndication;

namespace wallabag.DataModel
{
    public sealed class wallabagDataSource
    {
        private static wallabagDataSource _wallabagDataSource = new wallabagDataSource();
        public ApplicationSettings AppSettings { get { return ApplicationSettings.Instance; } }

        private ObservableDictionary _items = new ObservableDictionary();
        public ObservableDictionary Items { get { return this._items; } }

        public static async Task<ObservableDictionary> GetItemsAsync()
        {
            HttpClient http = new HttpClient();
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("WSSE", "profile=\"UsernameToken\"");
            http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            http.DefaultRequestHeaders.Add("X-WSSE", await Authentication.GetHeader());
            await http.GetStringAsync(new Uri("http://v2.wallabag.org/api/entries.json"));
            return new ObservableDictionary();
        }
        public static async Task<Item> GetItemAsync(string uniqueId)
        {
            await _wallabagDataSource.GetDataAsync(true);
            if (_wallabagDataSource.Items.ContainsKey(uniqueId))
            {
                return (Item)_wallabagDataSource.Items[uniqueId];
            }
            return null;
        }

        private async Task GetDataAsync(bool singleItem = false)
        {
            if (singleItem)
            {
                await RestoreItemsAsync();
                return;
            }
            Items.Clear();

            Windows.Web.Syndication.SyndicationClient client = new SyndicationClient();
            string[] parameters = new string[] { "home", "fav", "archive" };

            await GetItemsAsync();
        }

        private async Task SaveItemsAsync()
        {
            MemoryStream sessionData = new MemoryStream();
            DataContractSerializer serializer = new DataContractSerializer(typeof(ObservableDictionary), new List<Type>() { typeof(Item) });
            serializer.WriteObject(sessionData, Items);

            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync("items.xml", CreationCollisionOption.ReplaceExisting);
            using (Stream fileStream = await file.OpenStreamForWriteAsync())
            {
                sessionData.Seek(0, SeekOrigin.Begin);
                await sessionData.CopyToAsync(fileStream);
            }
        }
        private async Task RestoreItemsAsync()
        {
            try
            {
                var _temp = new Dictionary<string, object>();
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("items.xml");

                using (IInputStream inStream = await file.OpenSequentialReadAsync())
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(Dictionary<string, object>), new List<Type>() { typeof(Item) });
                    _temp = (Dictionary<string, object>)serializer.ReadObject(inStream.AsStreamForRead());
                }
                Items.Clear();
                foreach (var i in _temp)
                    Items.Add(i.Key, i.Value);
            }
            catch (FileNotFoundException)
            {
                System.Diagnostics.Debug.WriteLine("File not found.");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
        }
    }
}
