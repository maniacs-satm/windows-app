using System;
using System.Threading.Tasks;
using wallabag.Common;
using wallabag.DataModel;

namespace wallabag.ViewModels
{
    public class MainViewModel
    {
        public RelayCommand refreshCommand { get; private set; }
        private async Task RefreshItems()
        {
            var items = await wallabagDataSource.GetItemsAsync();
        }

        public MainViewModel()
        {
            refreshCommand = new RelayCommand(async () => await RefreshItems());
            refreshCommand.Execute(0);
        }
    }
}