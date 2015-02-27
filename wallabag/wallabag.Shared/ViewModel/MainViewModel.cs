using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using wallabag.Common;
using wallabag.Models;
using Windows.Networking.Connectivity;
using Windows.Web.Syndication;

namespace wallabag.ViewModel
{
    public class MainViewModel : viewModelBase
    {
        private ObservableCollection<ItemViewModel> _Items = new ObservableCollection<ItemViewModel>();
        public ObservableCollection<ItemViewModel> Items { get { return _Items; } }

        public ObservableCollection<ItemViewModel> unreadItems
        {
            get { return new ObservableCollection<ItemViewModel>(Items.Where(i => i.IsRead == false && i.IsFavourite == false)); }
        }
        public ObservableCollection<ItemViewModel> favouriteItems
        {
            get { return new ObservableCollection<ItemViewModel>(Items.Where(i => i.IsRead == false && i.IsFavourite == true)); }
        }
        public ObservableCollection<ItemViewModel> archivedItems
        {
            get { return new ObservableCollection<ItemViewModel>(Items.Where(i => i.IsRead == true)); }
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

        public RelayCommand refreshCommand { get; private set; }
        private async Task RefreshItems()
        {
            if (everythingFine)
            {
                StatusText = Helpers.LocalizedString("UpdatingText");
                IsActive = true;

                Items.Clear();

                // The SyndicationClient is the class that will be used for accessing RSS feeds.
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
                                if (!Items.Contains(new ItemViewModel(tmpItem)))
                                {
                                    Items.Add(new ItemViewModel(tmpItem));
                                }
                            }
                        }
                        IsActive = false;

                        // Inform the view that the item collections had changed.
                        RaisePropertyChanged(() => unreadItems);
                        RaisePropertyChanged(() => favouriteItems);
                        RaisePropertyChanged(() => archivedItems);
                    }
                    catch
                    {
                        IsActive = false;
                    }
                }
            }
        }

        public MainViewModel()
        {
            refreshCommand = new RelayCommand(async () => await RefreshItems());

            if (AppSettings["refreshOnStartup", false] || !everythingFine)
                refreshCommand.Execute(0);
        }
    }
}