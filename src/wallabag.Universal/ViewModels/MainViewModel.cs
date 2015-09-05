using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
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
        public DateTimeOffset MaxDate { get; } = DateTimeOffset.Now;

        public ObservableCollection<ItemViewModel> Items { get; set; } = new ObservableCollection<ItemViewModel>();
        public ObservableCollection<Tag> Tags { get; set; } = new ObservableCollection<Tag>();
        public ObservableCollection<string> DomainNames { get; set; } = new ObservableCollection<string>();
        public FilterProperties LastUsedFilterProperties { get; set; } = new FilterProperties();
        public bool IsSyncing { get; set; } = false;

        #region Tasks & Commands
        public async Task LoadItemsAsync()
        {
            Items.Clear();
            foreach (Item i in await DataService.GetItemsAsync(LastUsedFilterProperties))
                Items.Add(new ItemViewModel(i));

            Tags = new ObservableCollection<Tag>(await DataService.GetTagsAsync());
            foreach (var item in Items)
                if (!DomainNames.Contains(item.Model.DomainName))
                    DomainNames.Add(item.Model.DomainName);

            DomainNames = new ObservableCollection<string>(DomainNames.OrderBy(d => d).ToList());
        }

        public Command RefreshCommand { get; private set; }
        public Command NavigateToSettingsPageCommand { get; private set; }

        public Command FilterCommand { get; private set; }
        public Command ResetCommand { get; private set; }
        #endregion

        public override async void OnNavigatedTo(string parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (state.ContainsKey(nameof(LastUsedFilterProperties)))
                LastUsedFilterProperties = JsonConvert.DeserializeObject<FilterProperties>((string)state[nameof(LastUsedFilterProperties)]);
            else LastUsedFilterProperties = new FilterProperties();

            await LoadItemsAsync();
        }
        public override Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            state[nameof(LastUsedFilterProperties)] = JsonConvert.SerializeObject(LastUsedFilterProperties);
            return base.OnNavigatedFromAsync(state, suspending);
        }

        public MainViewModel()
        {
            RefreshCommand = new Command(async () =>
            {
                IsSyncing = true;
                await DataService.SyncWithServerAsync();
                await LoadItemsAsync();
                IsSyncing = false;
            });
            NavigateToSettingsPageCommand = new Command(() =>
            {
                Services.NavigationService.NavigationService.ApplicationNavigationService.Navigate(typeof(Views.SettingsPage));
            });

            FilterCommand = new Command(async () =>
            {
                await LoadItemsAsync();
            });
            ResetCommand = new Command(async () =>
            {
                LastUsedFilterProperties = new FilterProperties();
                await LoadItemsAsync();
            });
        }
    }
}