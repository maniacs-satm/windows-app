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

        public RelayCommand refreshCommand { get; private set; }
        private async Task RefreshItems()
        {
            StatusText = Helpers.LocalizedString("UpdatingText");
            IsActive = true;

            var items = await wallabagDataSource.GetItemsAsync();

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