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
        public string Url { get; set; }
        public ObservableCollection<Tag> Tags { get; set; }

        public DelegateCommand AddItemCommand { get; private set; }
        public DelegateCommand CancelCommand { get; private set; }

        public AddItemPageViewModel()
        {
            Tags = new ObservableCollection<Tag>();
            AddItemCommand = new DelegateCommand(async () =>
            {
                await DataService.AddItemAsync(Url, Tags.ToCommaSeparatedString());
                Url = string.Empty;
                Tags.Clear();

                if (NavigationService.CanGoBack)
                    NavigationService.GoBack();
            });
            CancelCommand = new DelegateCommand(() =>
            {
                Url = string.Empty;
                Tags.Clear();
                if (NavigationService.CanGoBack)
                    NavigationService.GoBack();
            });
        }
    }
}
