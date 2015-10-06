using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using wallabag.Common;
using wallabag.Models;
using wallabag.Services;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace wallabag.Views
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet werden kann oder auf die innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class AddItemPage : Page
    {
        public ViewModels.AddItemPageViewModel ViewModel { get { return (ViewModels.AddItemPageViewModel)DataContext; } }

        private ShareOperation shareOperation;

        public string Url { get; set; }
        public ICollection<Tag> Tags { get; set; }

        public DelegateCommand AddItemCommand { get; private set; }
        public DelegateCommand CancelCommand { get; private set; }

        public AddItemPage()
        {
            InitializeComponent();

            Url = string.Empty;
            Tags = new ObservableCollection<Tag>();

            AddItemCommand = new DelegateCommand(async () =>
            {
                addItemAppBarButton.IsEnabled = false;
                urlTextBox.IsEnabled = false;
                tagControl.IsEnabled = false;
                savingIndicator.Visibility = Windows.UI.Xaml.Visibility.Visible;

                await DataService.AddItemAsync(Url, Tags.ToCommaSeparatedString());

                Url = string.Empty;
                Tags.Clear();

                if (NavigationService.CanGoBack)
                    NavigationService.GoBack();
                else
                    shareOperation.ReportCompleted();
            });
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                shareOperation = (ShareOperation)e.Parameter;
                Url = (await shareOperation.Data.GetWebLinkAsync()).ToString();

                HideHeaderVisualState.StateTriggers.Add(new WindowsStateTriggers.DeviceFamilyStateTrigger() { DeviceFamily = WindowsStateTriggers.DeviceFamily.Desktop });
            }
        }
    }
}
