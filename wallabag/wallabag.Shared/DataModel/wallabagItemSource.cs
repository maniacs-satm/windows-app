using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using wallabag.Common;
using Windows.Data.Json;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
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

            string css = "body {" +
                CSSproperty("font-size", fontSize + "px") +
                CSSproperty("line-height", lineHeight.ToString().Replace(",", ".")) +
                //CSSproperty("color", tmpSettingsVM.textColor.Color) + // TODO
                //CSSproperty("background", tmpSettingsVM.Background.Color) +
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
            await _wallabagDataSource.GetDataAsync();
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
        private async Task GetDataAsync()
        {
            if (!everythingFine)
                return;

            Windows.Web.Syndication.SyndicationClient client = new SyndicationClient();
            string[] parameters = new string[] { "home", "fav", "archive" };

            foreach (string param in parameters) // perform the following step for each of the parameters (home, fav, archive)
            {
                Uri feedUri = new Uri(buildUrl(param));
                try
                {
                    SyndicationFeed feed = await client.RetrieveFeedAsync(feedUri);

                    if (feed.Items != null && feed.Items.Count > 0)
                    {
                        foreach (SyndicationItem item in feed.Items)
                        {
                            Item tmpItem = new Item();
                            if (item.Title != null && item.Title.Text != null)
                            {
                                tmpItem.Title = item.Title.Text;
                            }
                            if (item.Summary != null && item.Summary.Text != null)
                            {
                                tmpItem.Content = item.Summary.Text;
                            }
                            if (item.Links != null && item.Links.Count > 0)
                            {
                                tmpItem.Url = item.Links[0].Uri;
                            }
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
                            // to avoid duplicate items...
                            if (!Items.ContainsKey(tmpItem.UniqueId))
                                Items.Add(tmpItem.UniqueId, tmpItem);
                        }
                    }
                }
                catch
                {
                    return;
                }
            }
        }
    }
}
