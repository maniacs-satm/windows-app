using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using PropertyChanged;
using wallabag.Common.Mvvm;
using wallabag.DataModel;
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

        public ItemViewModel CurrentItem { get; set; }
        public bool CurrentItemIsNotNull { get { return CurrentItem != null; } }
        public bool IsMenuOpen { get; set; }
        #endregion

        #region Tasks & Commands
        private async Task LoadItems(FilterProperties FilterProperties)
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
            if (state.ContainsKey(nameof(IsMenuOpen)))
                IsMenuOpen = (bool)state[nameof(IsMenuOpen)];
            if (state.ContainsKey(nameof(CurrentItem.Model.Id)))
                CurrentItem = new ItemViewModel(await DataSource.GetItemAsync((int)state[nameof(CurrentItem.Model.Id)]));

            await LoadItems(new FilterProperties());
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            state[nameof(CurrentItem.Model.Id)] = CurrentItem.Model.Id;
            state[nameof(IsMenuOpen)] = IsMenuOpen;
            return base.OnNavigatedFromAsync(state, suspending);
        }

        public MainViewModel()
        {
            RefreshCommand = new Command(async () =>
            {
                await DataSource.RefreshItems();
                await LoadItems(new FilterProperties());
            });
            NavigateToSettingsPageCommand = new Command(() =>
            {
                if (NavigationService.CurrentPageType != typeof(Views.SettingsPage))
                    NavigationService.Navigate(typeof(Views.SettingsPage));
            });            
        }
    }
}