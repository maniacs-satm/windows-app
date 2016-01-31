using PropertyChanged;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Template10.Mvvm;
using wallabag.Common;
using wallabag.Data.Interfaces;
using wallabag.Models;
using Windows.ApplicationModel.DataTransfer.ShareTarget;

namespace wallabag.ViewModels
{
    [ImplementPropertyChanged]
    public class AddItemPageViewModel : ViewModelBase
    {
        private IDataService _dataService;

        public string Url { get; set; } = string.Empty;
        public ICollection<Tag> Tags { get; set; } = new ObservableCollection<Tag>();
        public bool IsActive { get; set; } = false;
        public ShareOperation ShareOperation { get; set; }

        public DelegateCommand AddItemCommand { get; private set; }
        public DelegateCommand CancelCommand { get; private set; }

        public AddItemPageViewModel(IDataService dataService)
        {
            _dataService = dataService;

            AddItemCommand = new DelegateCommand(async () =>
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
            });
            CancelCommand = new DelegateCommand(() =>
            {
                Url = string.Empty;
                Tags.Clear();

                if (ShareOperation != null)
                    ShareOperation.ReportCompleted();
                else if (NavigationService != null && NavigationService.CanGoBack)
                    NavigationService.GoBack();
            });
        }
    }
}
