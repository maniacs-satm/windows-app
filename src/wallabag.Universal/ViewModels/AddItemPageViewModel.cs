using GalaSoft.MvvmLight.Messaging;
using PropertyChanged;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Template10.Mvvm;
using wallabag.Common;
using wallabag.Data.Interfaces;
using wallabag.Models;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.UI.Xaml.Navigation;
using Template10.Services.NavigationService;

namespace wallabag.ViewModels
{
    [ImplementPropertyChanged]
    public class AddItemPageViewModel : ViewModelBase
    {
        private IDataService _dataService;

        public ShareOperation ShareOperation { get; set; }

        public string Url { get; set; } = string.Empty;
        public ICollection<Tag> Tags { get; set; } = new ObservableCollection<Tag>();
        public bool IsActive { get; set; } = false;

        public DelegateCommand AddItemCommand { get; private set; }
        public DelegateCommand CancelCommand { get; private set; }

        public AddItemPageViewModel(IDataService dataService)
        {
            _dataService = dataService;

            AddItemCommand = new DelegateCommand(async () => await AddItemAsync());
            CancelCommand = new DelegateCommand(() => Cancel());
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            Messenger.Default.Register<NotificationMessage<ShareOperation>>(this, message => { ShareOperation = message.Content; });
            return Task.CompletedTask;
        }
        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            Messenger.Default.Unregister<NotificationMessage<ShareOperation>>(this, message => { ShareOperation = message.Content; });
            return Task.CompletedTask;
        }

        private async Task AddItemAsync()
        {
            IsActive = true;
            await _dataService.AddItemAsync(Url, Tags.ToCommaSeparatedString());
            Url = string.Empty;
            Tags.Clear();
            IsActive = false;

            if (ShareOperation != null)
                ShareOperation.ReportCompleted();
            else if (NavigationService != null && NavigationService.CanGoBack)
                NavigationService.GoBack();
        }
        private void Cancel()
        {
            Url = string.Empty;
            Tags.Clear();

            if (ShareOperation != null)
                ShareOperation.ReportCompleted();
            else if (NavigationService != null && NavigationService.CanGoBack)
                NavigationService.GoBack();
        }
    }
}
