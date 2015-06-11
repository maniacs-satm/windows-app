using System.Collections.ObjectModel;
using System.Threading.Tasks;
using PropertyChanged;
using wallabag.Common.MVVM;
using wallabag.DataModel;

namespace wallabag.ViewModels
{
    [ImplementPropertyChanged]
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<ItemViewModel> Items { get; set; }

        public ItemViewModel CurrentItem { get; set; }
        public bool CurrentItemIsNotNull { get { return CurrentItem != null; } }

        private async Task LoadItems(DataSource.ItemType itemType)
        {
            IsActive = true;
            foreach (Item i in await DataSource.GetItemsAsync(itemType))
                Items.Add(new ItemViewModel(i));
            IsActive = false;
        }

        public RelayCommand RefreshCommand { get; private set; }
        public RelayCommand ShowUnreadItems { get; private set; }
        public RelayCommand ShowFavoriteItems { get; private set; }
        public RelayCommand ShowArchivedItems { get; private set; }

        public MainViewModel()
        {
            RefreshCommand = new RelayCommand(async () =>
            {
                await DataSource.RefreshItems();
                await LoadItems(DataSource.ItemType.Unread);
            });

            ShowUnreadItems = new RelayCommand(async () => await LoadItems(DataSource.ItemType.Unread));
            ShowFavoriteItems = new RelayCommand(async () => await LoadItems(DataSource.ItemType.Favorites));
            ShowArchivedItems = new RelayCommand(async () => await LoadItems(DataSource.ItemType.Archived));
        }
    }
}