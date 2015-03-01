using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using wallabag.Common;
using wallabag.DataModel;
using Windows.Networking.Connectivity;
using Windows.Web.Syndication;

namespace wallabag.ViewModel
{
    public class MainViewModel : viewModelBase
    {
        private ObservableCollection<Item> _Items = new ObservableCollection<Item>();
        public ObservableCollection<Item> Items { get { return _Items; } }

        public ObservableCollection<Item> unreadItems
        {
            get { return new ObservableCollection<Item>(Items.Where(i => i.IsRead == false && i.IsFavourite == false)); }
        }
        public ObservableCollection<Item> favouriteItems
        {
            get { return new ObservableCollection<Item>(Items.Where(i => i.IsRead == false && i.IsFavourite == true)); }
        }
        public ObservableCollection<Item> archivedItems
        {
            get { return new ObservableCollection<Item>(Items.Where(i => i.IsRead == true)); }
        }

        public RelayCommand refreshCommand { get; private set; }
        private async Task RefreshItems()
        {
            StatusText = Helpers.LocalizedString("UpdatingText");
            IsActive = true;

            var items = await wallabagDataSource.GetItemsAsync();
            foreach (Item i in items)
                Items.Add(i);

            IsActive = false;
        }

        public MainViewModel()
        {
            refreshCommand = new RelayCommand(async () => await RefreshItems());

            if (AppSettings["refreshOnStartup", false])
                refreshCommand.Execute(0);
        }
    }
}