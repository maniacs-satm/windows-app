using wallabag.DataModel;
using wallabag.ViewModels;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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
            if (e?.Parameter.GetType() == typeof(int))
            {
                DataContext = new SingleItemPageViewModel() { CurrentItem = await DataSource.GetItemAsync((int)e.Parameter) };
                ApplicationView.GetForCurrentView().Title = (DataContext as SingleItemPageViewModel).CurrentItem.Model.Title;
                WebView.NavigateToString((DataContext as SingleItemPageViewModel).CurrentItem.ContentWithHeader);
            }
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ApplicationView.GetForCurrentView().Title = string.Empty;
            base.OnNavigatedFrom(e);
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
        }
    }
}
