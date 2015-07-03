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
        private ObservableCollection<ItemViewModel> _Items = new ObservableCollection<ItemViewModel>();
        public ObservableCollection<ItemViewModel> Items
        {
            get { return _Items; }
            set { _Items = value; }
        }

        public ItemViewModel CurrentItem { get; set; }
        public bool CurrentItemIsNotNull { get { return CurrentItem != null; } }

        private async Task LoadItems(FilterProperties FilterProperties)
        {
            IsActive = true;
            Items.Clear();
            foreach (Item i in await DataSource.GetItemsAsync(FilterProperties))
                Items.Add(new ItemViewModel(i));
            IsActive = false;
        }

        public RelayCommand RefreshCommand { get; private set; }

        public MainViewModel()
        {
            RefreshCommand = new RelayCommand(async () =>
            {
                await DataSource.RefreshItems();
                await LoadItems(new FilterProperties());
            });
        }
    }
}