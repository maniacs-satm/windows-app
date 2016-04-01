using GalaSoft.MvvmLight.Messaging;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace wallabag.Views
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet werden kann oder auf die innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class FirstStartPage : Page
    {
        public ViewModels.FirstStartPageViewModel ViewModel { get { return (ViewModels.FirstStartPageViewModel)DataContext; } }

        public FirstStartPage()
        {
            this.InitializeComponent();

            if (ViewModel.CredentialsAreExisting)
                GoToStep3.Begin();
            else
                GoToStep0.Begin();
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;

            SystemNavigationManager.GetForCurrentView().BackRequested += (s, e) =>
            {
                if (Step2Panel.Visibility == Visibility.Visible)
                {
                    e.Handled = true;
                    Step2Panel.Visibility = Visibility.Collapsed;
                    SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                    GoToStep0.Begin();
                }
            };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Messenger.Default.Register<NotificationMessage>(this, message =>
            {
                if (message.Notification == "LoginFailed")
                {
                    GoToStep2.Begin();
                    RevertStep3.Begin();
                }
                else if (message.Notification == "LoginSucceded")
                    GoToStep4.Begin();
            });
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Messenger.Default.Unregister<NotificationMessage>(this);
        }

        private void framabagUserButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Set the framabag URL.
            //wallabagUrlTextBox.Text = "https://v2.wallabag.org/";

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;

            if (sender == framabagUserButton)
            {
                wallabagUrlTextBox.Visibility = Visibility.Collapsed;
                clientIdTextBox.Visibility = Visibility.Collapsed;
                clientSecretTextBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                wallabagUrlTextBox.Visibility = Visibility.Visible;
                clientIdTextBox.Visibility = Visibility.Visible;
                clientSecretTextBox.Visibility = Visibility.Visible;
            }

            GoToStep2.Begin();

            if (sender == framabagUserButton || ViewModel.CredentialsWereSynced)
                userNameTextBox.Focus(FocusState.Programmatic);
            else
                clientIdTextBox.Focus(FocusState.Programmatic);
        }
        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            GoToStep3.Begin();
        }
    }
}
