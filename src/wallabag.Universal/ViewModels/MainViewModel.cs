using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using PropertyChanged;
using wallabag.Common;
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
        public FilterProperties FilterProperties { get; set; } = new FilterProperties();

        #region Tasks & Commands
        public async Task LoadItemsAsync(FilterProperties FilterProperties)
        {
            Items.Clear();
            foreach (Item i in await DataService.GetItemsAsync(FilterProperties))
                Items.Add(new ItemViewModel(i));
        }

        public Command RefreshCommand { get; private set; }
        public Command NavigateToSettingsPageCommand { get; private set; }

        public Command FilterCommand { get; private set; }
        public Command ResetCommand { get; private set; }
        #endregion

        public override async void OnNavigatedTo(string parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (AppSettings.SyncOnStartup)
                await DataService.SyncWithServerAsync();

            await LoadItemsAsync(new FilterProperties());
            Tags = new ObservableCollection<Tag>(await DataService.GetTagsAsync());
            foreach (var item in Items)
                if (!DomainNames.Contains(item.Model.DomainName))
                    DomainNames.Add(item.Model.DomainName);

            if (FilterProperties != null)
                await LoadItemsAsync(FilterProperties);

            DomainNames = new ObservableCollection<string>(DomainNames.OrderBy(d => d).ToList());
        }

        public MainViewModel()
        {
            RefreshCommand = new Command(async () =>
            {
                await DataService.SyncWithServerAsync();
                await LoadItemsAsync(new FilterProperties());
                Tags = new ObservableCollection<Tag>(await DataService.GetTagsAsync());
            });
            NavigateToSettingsPageCommand = new Command(() =>
            {
                Services.NavigationService.NavigationService.ApplicationNavigationService.Navigate(typeof(Views.SettingsPage));
            });

            FilterCommand = new Command(async () =>
            {
                await LoadItemsAsync(FilterProperties);
            });
            ResetCommand = new Command(async () =>
            {
                FilterProperties = new FilterProperties() { ItemType = FilterProperties.FilterPropertiesItemType.Unread };
                await LoadItemsAsync(FilterProperties);
            });
        }
    }
}