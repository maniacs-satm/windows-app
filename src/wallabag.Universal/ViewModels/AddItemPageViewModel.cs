using GalaSoft.MvvmLight.Messaging;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Template10.Mvvm;
using wallabag.Common;
using wallabag.Data.Interfaces;
using wallabag.Models;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.UI.Xaml.Navigation;

namespace wallabag.ViewModels
{
    [ImplementPropertyChanged]
    public class AddItemPageViewModel : ViewModelBase
    {
        private IDataService _dataService;

        public ShareOperation ShareOperation { get; set; } 

        public string Url { get; set; } = string.Empty;
        public ICollection<Tag> Tags { get; set; } = new ObservableCollection<Tag>();

        public DelegateCommand AddItemCommand { get; private set; }
        public DelegateCommand CancelCommand { get; private set; }

        public AddItemPageViewModel(IDataService dataService)
        {
            _dataService = dataService;

            AddItemCommand = new DelegateCommand(async () => await AddItemAsync());
            CancelCommand = new DelegateCommand(() => Cancel());
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (SessionState.ContainsKey("ShareOperation"))
            {
                this.ShareOperation = SessionState.Get<ShareOperation>("ShareOperation");
                this.Url = (await ShareOperation.Data.GetWebLinkAsync()).ToString();
                SessionState.Remove("ShareOperation");
            }
        }

        private async Task AddItemAsync()
        {
            // This message is handled if you share a link over the DataTransferManager.
            Messenger.Default.Send(new NotificationMessage("StartAnimation"));

            await _dataService.AddItemAsync(Url, Tags.ToCommaSeparatedString());

            // This message is handled if you add a link over the AddItemDialog.
            Messenger.Default.Send(new NotificationMessage("UpdateView"));

            Url = string.Empty;
            Tags.Clear();

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
