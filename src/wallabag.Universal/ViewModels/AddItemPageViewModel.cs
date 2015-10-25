using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using PropertyChanged;
using Template10.Mvvm;
using wallabag.Common;
using wallabag.Models;
using wallabag.Services;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.UI.Xaml.Navigation;

namespace wallabag.ViewModels
{
    [ImplementPropertyChanged]
    public class AddItemPageViewModel : ViewModelBase
    {
        public string Url { get; set; } = string.Empty;
        public ICollection<Tag> Tags { get; set; } = new ObservableCollection<Tag>();
        public bool IsActive { get; set; } = false;
        public ShareOperation ShareOperation { get; set; }

        public DelegateCommand AddItemCommand { get; private set; }
        public DelegateCommand CancelCommand { get; private set; }

        public AddItemPageViewModel()
        {
            AddItemCommand = new DelegateCommand(async () =>
            {
                IsActive = true;
                await DataService.AddItemAsync(Url, Tags.ToCommaSeparatedString());
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
