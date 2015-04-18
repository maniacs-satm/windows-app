using PropertyChanged;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using wallabag.Common;
using wallabag.DataModel;
using System.Linq;

namespace wallabag.ViewModels
{
    [ImplementPropertyChanged]
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<ItemViewModel> Items { get; set; }
        public ObservableCollection<ItemViewModel> CurrentItems { get; set; }
        private ObservableCollection<ItemViewModel> UnreadItems
        {
            get
            {
                return new ObservableCollection<ItemViewModel>(Items?.Where(x => x.Model.IsStarred == false &&
                    x.Model.IsArchived == false &&
                    x.Model.IsDeleted == false));
            }
        }
        private ObservableCollection<ItemViewModel> FavoriteItems
        {
            get
            {
                return new ObservableCollection<ItemViewModel>(Items?.Where(x => x.Model.IsStarred == true &&
                    x.Model.IsDeleted == false));
            }
        }
        private ObservableCollection<ItemViewModel> ArchivedItems
        {
            get
            {
                return new ObservableCollection<ItemViewModel>(Items?.Where(x => x.Model.IsArchived == true &&
                    x.Model.IsDeleted == false));
            }
        }

        public ItemViewModel CurrentItem { get; set; }
        public bool CurrentItemIsNotNull { get { return CurrentItem != null; } }

        public RelayCommand RefreshCommand { get; set; }
        private async Task Refresh()
        {
            IsActive = true;
            if (await DataSource.GetItemsAsync())
            {
                foreach (var item in DataSource.Items)
                    Items.Add((ItemViewModel)item.Value);
                RaisePropertyChanged("Items");
                RaisePropertyChanged("CurrentItems");
                RaisePropertyChanged("UnreadItems");
                RaisePropertyChanged("FavoriteItems");
                RaisePropertyChanged("ArchivedItems");
            }
            IsActive = false;
            if (CurrentItems == null)
                CurrentItems = UnreadItems;
        }

        public RelayCommand DeleteCommand { get; set; }
        private async Task Delete()
        {
            bool success = await CurrentItem.Delete();
            if (success)
            {
                Items.Remove(CurrentItem);
                RaisePropertyChanged("CurrentItems");
                RaisePropertyChanged("UnreadItems");
                RaisePropertyChanged("FavoriteItems");
                RaisePropertyChanged("ArchivedItems");
                CurrentItem = null;
            }

        }

        public RelayCommand ShowUnreadItems { get; set; }
        public RelayCommand ShowFavoriteItems { get; set; }
        public RelayCommand ShowArchivedItems { get; set; }

        public MainViewModel()
        {
            Items = new ObservableCollection<ItemViewModel>();
            RefreshCommand = new RelayCommand(async () => await Refresh());
            DeleteCommand = new RelayCommand(async () => await Delete());

            ShowUnreadItems = new RelayCommand(() => { CurrentItems = UnreadItems; });
            ShowFavoriteItems = new RelayCommand(() => { CurrentItems = FavoriteItems; });
            ShowArchivedItems = new RelayCommand(() => { CurrentItems = ArchivedItems; });
        }
    }
}