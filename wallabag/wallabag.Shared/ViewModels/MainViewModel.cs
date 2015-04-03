using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using wallabag.Common;
using wallabag.DataModel;
using PropertyChanged;

namespace wallabag.ViewModels
{
    [ImplementPropertyChanged]
    public class MainViewModel
    {
        public ObservableCollection<ItemViewModel> Items { get; set; }

        public RelayCommand RefreshCommand { get; set; }
        private async Task Refresh()
        {
            // TODO: Improve the method handling and show some progress on the UI.
            await DataSource.GetItemsAsync();
            Items = DataSource.Items;
        }

        public MainViewModel()
        {
            RefreshCommand = new RelayCommand(async () => await Refresh());
        }
    }
}