using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using PropertyChanged;
using wallabag.Common.Mvvm;
using wallabag.Models;
using wallabag.Services;
using Windows.UI.Xaml.Navigation;

namespace wallabag.ViewModels
{
    [ImplementPropertyChanged]
    public class MainViewModel : ViewModelBase
    {
        public override string ViewModelIdentifier { get; set; } = "MainViewModel";

        #region Properties
        private ObservableCollection<ItemViewModel> _Items = new ObservableCollection<ItemViewModel>();
        public ObservableCollection<ItemViewModel> Items
        {
            get { return _Items; }
            set { _Items = value; }
        }

        #endregion

        #region Tasks & Commands
        private async Task LoadItemsAsync(FilterProperties FilterProperties)
        {
            Items.Clear();
            foreach (Item i in await DataSource.GetItemsAsync(FilterProperties))
                Items.Add(new ItemViewModel(i));
        }

        public Command RefreshCommand { get; private set; }
        public Command NavigateToSettingsPageCommand { get; private set; }
        #endregion

        public override async void OnNavigatedTo(string parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            await LoadItemsAsync(new FilterProperties());
        }

        public MainViewModel()
        {
            RefreshCommand = new Command(async () =>
            {
                await DataSource.LoadItemsAsync();
                await LoadItemsAsync(new FilterProperties());
            });
            NavigateToSettingsPageCommand = new Command(() =>
            {
                Services.NavigationService.NavigationService.ApplicationNavigationService.Navigate(typeof(Views.SettingsPage));
            });
        }
    }
}