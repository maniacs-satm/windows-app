﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using wallabag.Common;
using wallabag.ViewModel;
using Windows.Data.Xml.Dom;
using Windows.Networking.Connectivity;
using Windows.Security.Cryptography.Certificates;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Web.Syndication;

namespace wallabag.DataModel
{
    public class Item
    {
        public ApplicationSettings AppSettings { get { return ApplicationSettings.Instance; } }

        public Item()
        {
            UniqueId = Guid.NewGuid().ToString();
        }
        public Item(String uniqueId, String title, String content, Uri url, bool isRead, bool isFavourite)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Content = content;
            this.Url = url;
            this.IsRead = isRead;
            this.IsFavourite = isFavourite;
        }

        public string UniqueId { get; set; }
        private string _title;
        public string Title
        {
            get
            {
                // Regular expression to remove multiple whitespaces (including newline etc.) in title.
                Regex r = new Regex("\\s+");
                return r.Replace(_title, " ");
            }
            set { _title = value; }
        }
        public string Content { get; set; }
        public string ContentWithTitle
        {
            get
            {
                var content =
                    "<html><head><link rel=\"stylesheet\" href=\"ms-appx-web:///Assets/css/wallabag.css\" type=\"text/css\" media=\"screen\" />" + CSS() + "</head>" +
                        "<h1 class=\"wallabag-header\">" + Title + "</h1>" +
                        this.Content +
                    "</html>";
                return content;
            }
        }
        public Uri Url { get; set; }
        public bool IsRead { get; set; }
        public bool IsFavourite { get; set; }

        private string CSSproperty(string name, object value)
        {
            if (value.GetType() != typeof(Color))
            {
                return string.Format("{0}: {1};", name, value.ToString());
            }
            else
            {
                var color = (Color)value;
                var tmpColor = string.Format("rgba({0}, {1}, {2}, {3})", color.R, color.G, color.B, color.A);
                return string.Format("{0}: {1};", name, tmpColor);
            }
        }
        private string CSS()
        {
            double fontSize = AppSettings["fontSize", 18];
            double lineHeight = AppSettings["lineHeight", 1.5];

            SettingsViewModel tmpSettingsVM = new SettingsViewModel();

            string css = "body {" +
                CSSproperty("font-size", fontSize + "px") +
                CSSproperty("line-height", lineHeight.ToString().Replace(",", ".")) +
                CSSproperty("color", tmpSettingsVM.textColor.Color) +
                CSSproperty("background", tmpSettingsVM.Background.Color) +
#if WINDOWS_APP
 CSSproperty("max-width", "960px") +
                CSSproperty("margin", "0 auto") +
                CSSproperty("padding", "0 20px") +
#endif
 "}";
            return "<style>" + css + "</style>";
        }

        public override string ToString()
        {
            return this.Title;
        }
    }

    public sealed class wallabagDataSource
    {
        private static wallabagDataSource _wallabagDataSource = new wallabagDataSource();
        public ApplicationSettings AppSettings { get { return ApplicationSettings.Instance; } }

        private ObservableDictionary _items = new ObservableDictionary();
        public ObservableDictionary Items { get { return this._items; } }

        public static async Task<ObservableDictionary> GetItemsAsync()
        {
            await _wallabagDataSource.GetDataAsync();
            return _wallabagDataSource._items;
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

        private bool everythingFine
        {
            get
            {
                string wallabagUrl = AppSettings["wallabagUrl", string.Empty];
                int userId = AppSettings["userId", 1];
                string token = AppSettings["Token", string.Empty];

                ConnectionProfile connections = NetworkInformation.GetInternetConnectionProfile();
                bool internet = connections != null && connections.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;

                return wallabagUrl != string.Empty && userId != 0 && token != string.Empty && internet;
            }
        }
        private string buildUrl(string parameter)
        {
            string wallabagUrl = AppSettings["wallabagUrl", string.Empty];
            int userId = AppSettings["userId", 1];
            string token = AppSettings["Token", string.Empty];

            if (everythingFine)
                return string.Format("{0}?feed&type={1}&user_id={2}&token={3}", wallabagUrl, parameter, userId, token);

            return string.Empty;
        }
        private async Task GetDataAsync(bool singleItem = false)
        {
            if (!everythingFine || singleItem)
            {
                await RestoreItemsAsync();
                return;
            }
            Items.Clear();

            Windows.Web.Syndication.SyndicationClient client = new SyndicationClient();
            string[] parameters = new string[] { "home", "fav", "archive" };

            foreach (string param in parameters) // perform the following step for each of the parameters (home, fav, archive)
            {
                Uri feedUri = new Uri(buildUrl(param));
                try
                {
                    var filter = new HttpBaseProtocolFilter();

                    if (AppSettings["AllowSelfSignedCertificates"] == true)
                    {
                        filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.IncompleteChain);
                        filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.Expired);
                        filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.Untrusted);
                        filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.InvalidName);
                    }

                    var httpClient = new HttpClient(filter);
                    string rssString = await httpClient.GetStringAsync(feedUri);

                    XmlDocument rssDocument = new XmlDocument();
                    rssDocument.LoadXml(rssString);

                    var items = rssDocument.GetElementsByTagName("item");
                    if (items.Count > 0)
                    {
                        foreach (var item in items)
                        {
                            Item tmpItem =new Item();
                            tmpItem.Title = item.ChildNodes.Where(c => c.NodeName == "title").First().InnerText;
                            tmpItem.Url = new Uri(item.ChildNodes.Where(c => c.NodeName == "link").First().InnerText);
                            tmpItem.Content = item.ChildNodes.Where(c => c.NodeName == "description").First().InnerText;

                            switch (param)
                            {
                                // If we are in the 'fav' loop, set the IsFavourite property to 'true'.
                                case "fav":
                                    tmpItem.IsFavourite = true;
                                    break;

                                // If we are in the 'archive' loop, set the IsRead property to 'true'.
                                case "archive":
                                    tmpItem.IsRead = true;
                                    break;
                            }
                            Items.Add(tmpItem.UniqueId, tmpItem);
                        }
                    }
                    await SaveItemsAsync();
                }
                catch
                {
                    return;
                }
            }
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
