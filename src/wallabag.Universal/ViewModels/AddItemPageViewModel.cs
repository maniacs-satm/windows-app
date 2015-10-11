using System.Collections.ObjectModel;
using PropertyChanged;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using wallabag.Common;
using wallabag.Models;
using wallabag.Services;

namespace wallabag.ViewModels
{
    [ImplementPropertyChanged]
    public class AddItemPageViewModel : ViewModelBase
    {
        public string Url { get; set; } = string.Empty;
        public ObservableCollection<Tag> Tags { get; set; } = new ObservableCollection<Tag>();

        public DelegateCommand AddItemCommand { get; private set; }
        public DelegateCommand CancelCommand { get; private set; }

        public AddItemPageViewModel()
        {
            AddItemCommand = new DelegateCommand(async () =>
            {
                await DataService.AddItemAsync(Url, Tags.ToCommaSeparatedString());
                Url = string.Empty;
                Tags.Clear();

                if (NavigationService != null && NavigationService.CanGoBack)
                    NavigationService.GoBack();
            });
            CancelCommand = new DelegateCommand(() =>
            {
                Url = string.Empty;
                Tags.Clear();
                if (NavigationService != null && NavigationService.CanGoBack)
                    NavigationService.GoBack();
            });
        }
    }
}
