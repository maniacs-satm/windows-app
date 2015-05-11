using wallabag.DataModel;
using wallabag.ViewModels;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace wallabag.Views
{
    public sealed partial class SingleItemPage : Common.basicPage
    {
        public SingleItemPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e?.Parameter.GetType() == typeof(int))
            {
                DataContext = new SingleItemPageViewModel() { CurrentItem = await DataSource.GetItemAsync((int)e.Parameter) };
                ApplicationView.GetForCurrentView().Title = (DataContext as SingleItemPageViewModel).CurrentItem.Model.Title;
                WebView.NavigateToString((DataContext as SingleItemPageViewModel).CurrentItem.ContentWithHeader);
            }
            SystemNavigationManager.GetForCurrentView().BackRequested += SingleItemPage_BackRequested;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            ApplicationView.GetForCurrentView().Title = string.Empty;
            SystemNavigationManager.GetForCurrentView().BackRequested -= SingleItemPage_BackRequested;
        }

        private void OnBackRequested()
        {
            Frame.GoBack();
        }

        private void SingleItemPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;
            OnBackRequested();
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            OnBackRequested();
        }
    }
}
