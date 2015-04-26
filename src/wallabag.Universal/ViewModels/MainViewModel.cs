using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using wallabag.Common;
using wallabag.DataModel;

namespace wallabag.ViewModels
{
    [ImplementPropertyChanged]
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<ItemViewModel> Items { get { return DataSource.Items; } }

        public ItemViewModel CurrentItem { get; set; }
        public bool CurrentItemIsNotNull { get { return CurrentItem != null; } }
        
        public RelayCommand RefreshCommand { get; private set; }
        private async Task Refresh()
        {
            IsActive = true;
            await DataSource.GetItemsAsync();
            RaisePropertyChanged(nameof(Items));
            IsActive = false;
        }

        public RelayCommand DeleteCommand { get; private set; }
        private async Task Delete()
        {
            bool success = await CurrentItem.Delete();
            if (success)
            {
                Items.Remove(CurrentItem);
                CurrentItem = null;
            }

        }

        public RelayCommand ShowUnreadItems { get; private set; }
        public RelayCommand ShowFavoriteItems { get; private set; }
        public RelayCommand ShowArchivedItems { get; private set; }

        public MainViewModel()
        {
            RefreshCommand = new RelayCommand(async () => await Refresh());
            DeleteCommand = new RelayCommand(async () => await Delete());
        }
    }
}