using System;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// Die Elementvorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace wallabag.Views
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet werden kann oder auf die innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class FirstStartPage : Page
    {
        public FirstStartPage()
        {
            this.InitializeComponent();
            GoToStep0.Begin();
            GoToStep0.Completed += (s, e) =>
            {
                if (string.IsNullOrEmpty(Common.AppSettings.AccessToken))
                    GoToStep0.Begin();
                else
                    GoToStep3.Begin();
            };
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;

            SystemNavigationManager.GetForCurrentView().BackRequested += (s, e) =>
            {
                if (Step2Panel.Visibility == Visibility.Visible)
                {
                    e.Handled = true;
                    Step2Panel.Visibility = Visibility.Collapsed;
                    Step3Panel.Visibility = Visibility.Collapsed;
                    SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                    GoToStep0.Begin();
                }
            };
        }

        private void framabagUserButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Set the framabag URL.
            //wallabagUrlTextBox.Text = "https://v2.wallabag.org/";

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;

            if (sender == framabagUserButton)
                wallabagUrlTextBox.Visibility = Visibility.Collapsed;
            else
                wallabagUrlTextBox.Visibility = Visibility.Visible;

            GoToStep2.Begin();

            if (sender == notFramabagUserButton)
                wallabagUrlTextBox.Focus(FocusState.Programmatic);
            else
                userNameTextBox.Focus(FocusState.Programmatic);
        }

        private async void imageCreditButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://www.flickr.com/photos/oneterry/16711663295/"));
        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            GoToStep3.Begin();
        }
    }
}
