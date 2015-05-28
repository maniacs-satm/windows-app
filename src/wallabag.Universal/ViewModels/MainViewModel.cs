using PropertyChanged;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using wallabag.Common.MVVM;
using wallabag.DataModel;

namespace wallabag.ViewModels
{
    [ImplementPropertyChanged]
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<ItemViewModel> _Items { get { return DataSource.Items; } }
        public ObservableCollection<ItemViewModel> Items { get; set; }

        public ItemViewModel CurrentItem { get; set; }
        public bool CurrentItemIsNotNull { get { return CurrentItem != null; } }

        public RelayCommand RefreshCommand { get; private set; }
        private async Task Refresh()
        {
            IsActive = true;
            if (await DataSource.GetItemsAsync())
            {
                RaisePropertyChanged(nameof(Items));
                ShowUnreadItems.Execute(0);
            }
            IsActive = false;
        }

        public RelayCommand DeleteCommand { get; private set; }
        private async Task Delete()
        {
            bool success = await CurrentItem.Delete();
            if (success)
            {
                _Items.Remove(CurrentItem);
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

            ShowUnreadItems = new RelayCommand(() => { if (_Items != null) Items = new ObservableCollection<ItemViewModel>(_Items.Where(i => i.Model.IsArchived == false && i.Model.IsDeleted == false && i.Model.IsStarred == false)); });
            ShowFavoriteItems = new RelayCommand(() => { if (_Items != null) Items = new ObservableCollection<ItemViewModel>(_Items.Where(i => i.Model.IsStarred == true && i.Model.IsDeleted == false)); });
            ShowArchivedItems = new RelayCommand(() => { if (_Items != null) Items = new ObservableCollection<ItemViewModel>(_Items.Where(i => i.Model.IsArchived == true && i.Model.IsDeleted == false)); });
        }
    }
}