using System;
using Windows.System;
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
            FirstStartAnimation.Begin();
        }

        private void framabagUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender == framabagUserButton)
            {
                wallabagUrlTextBox.Visibility = Visibility.Collapsed;
                wallabagUrlTextBox.Text = "https://v2.wallabag.org/"; // TODO: Set the framabag url.
            }

            StackPanelChangeStoryboard.Begin();

            if (sender == notFramabagUserButton)
                wallabagUrlTextBox.Focus(FocusState.Programmatic);
            else
                userNameTextBox.Focus(FocusState.Programmatic);
        }

        private async void imageCreditButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://www.flickr.com/photos/oneterry/16711663295/"));
        }
    }
}
