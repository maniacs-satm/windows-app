using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using wallabag.Common;
using Windows.Data.Json;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Syndication;

namespace wallabag.DataModel
{
    public class Item
    {
        public Item()
        {
            UniqueId = new Guid("zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz").ToString();
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
        public string Title { get; set; }
        public string Content { get; set; }
        public Uri Url { get; set; }
        public bool IsRead { get; set; }
        public bool IsFavourite { get; set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    public sealed class wallabagDataSource
    {
        private static wallabagDataSource _wallabagDataSource = new wallabagDataSource();
        public ApplicationSettings AppSettings { get { return ApplicationSettings.Instance; } }

        private ObservableCollection<Item> _items;
        public ObservableCollection<Item> Items { get { return this._items; } }

        public static async Task<IEnumerable<Item>> GetItemsAsync()
        {
            await _wallabagDataSource.GetDataAsync();
            return _wallabagDataSource._items;
        }
        public static async Task<Item> GetItemAsync(string uniqueId)
        {
            await _wallabagDataSource.GetDataAsync();
            var matches = _wallabagDataSource.Items.Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
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
            if (this._items.Count != 0 || !everythingFine)
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
                            if (!Items.Contains(tmpItem))
                                Items.Add(tmpItem);
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
