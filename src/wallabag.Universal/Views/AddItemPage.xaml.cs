using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System;

// Die Elementvorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace wallabag.Views
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet werden kann oder auf die innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class AddItemPage : Page
    {
        public ViewModels.AddItemPageViewModel ViewModel { get { return (ViewModels.AddItemPageViewModel)DataContext; } }

        public AddItemPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                ViewModel.ShareOperation = (ShareOperation)e.Parameter;
                ViewModel.Url = (await ViewModel.ShareOperation.Data.GetWebLinkAsync()).ToString();
            }

            HideHeaderVisualState.StateTriggers.Add(new WindowsStateTriggers.DeviceFamilyStateTrigger() { DeviceFamily = WindowsStateTriggers.DeviceFamily.Desktop });
        }
    }
}
