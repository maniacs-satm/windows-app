using GalaSoft.MvvmLight.Messaging;
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

        public AddItemPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            HideHeaderVisualState.StateTriggers.Add(new WindowsStateTriggers.DeviceFamilyStateTrigger() { DeviceFamily = WindowsStateTriggers.DeviceFamily.Desktop });
            Messenger.Default.Register<NotificationMessage>(this, message =>
            {
                if (message.Notification == "StartAnimation")
                    StartSavingStoryboard.Begin();
            });
        }
    }
}
