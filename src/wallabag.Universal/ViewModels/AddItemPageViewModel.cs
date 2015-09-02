using System.Collections.ObjectModel;
using PropertyChanged;
using wallabag.Common;
using wallabag.Common.Mvvm;
using wallabag.Models;
using wallabag.Services;

namespace wallabag.ViewModels
{
    [ImplementPropertyChanged]
    public class AddItemPageViewModel
    {
        public string Url { get; set; }
        public ObservableCollection<Tag> Tags { get; set; }

        public Command AddItemCommand { get; private set; }
        public Command CancelCommand { get; private set; }

        public AddItemPageViewModel()
        {
            Tags = new ObservableCollection<Tag>();
            AddItemCommand = new Command(async () =>
            {
                await DataService.AddItemAsync(Url, Tags.ToCommaSeparatedString());
                Url = string.Empty;
                Tags.Clear();

                if (Services.NavigationService.NavigationService.ApplicationNavigationService.CanGoBack)
                    Services.NavigationService.NavigationService.ApplicationNavigationService.GoBack();
            });
            CancelCommand = new Command(() =>
            {
                Url = string.Empty;
                Tags.Clear();
                if (Services.NavigationService.NavigationService.ApplicationNavigationService.CanGoBack)
                    Services.NavigationService.NavigationService.ApplicationNavigationService.GoBack();
            });
        }
    }
}
