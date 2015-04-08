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
    public class MainViewModel
    {
        public ObservableCollection<ItemViewModel> Items { get; set; }
        public ItemViewModel CurrentItem { get; set; }

        public RelayCommand RefreshCommand { get; set; }
        private async Task Refresh()
        {
            // TODO: Improve the method handling and show some progress on the UI.
            if (await DataSource.GetItemsAsync())
                foreach (var item in DataSource.Items)
                    Items.Add((ItemViewModel)item.Value);
        }

        public RelayCommand DeleteCommand { get; set; }
        private async Task Delete()
        {
            bool success = await CurrentItem.Delete();
            if (success)
                Items.Remove(CurrentItem);
        }

        public MainViewModel()
        {
            Items = new ObservableCollection<ItemViewModel>();
            RefreshCommand = new RelayCommand(async () => await Refresh());
            DeleteCommand = new RelayCommand(async () => await Delete());
        }
    }
}