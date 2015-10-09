using wallabag.Common;
using wallabag.ViewModels;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace wallabag.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPageViewModel ViewModel { get { return (SettingsPageViewModel)DataContext; } }

        public SettingsPage()
        {
            InitializeComponent();
        }

        private void IntervalRadioButton_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
            => AppSettings.BackgroundTaskInterval = (uint)(sender as RadioButton).Content;
    }
}
