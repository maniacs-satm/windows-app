using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace wallabag.Views
{
    public sealed partial class SingleItemPage : Common.basicPage
    {
        public SingleItemPage()
        {
            this.InitializeComponent();
        }
        
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
                DataContext = new ViewModels.SingleItemPageViewModel() { CurrentItem = await wallabag.DataModel.DataSource.GetItemAsync((int)e.Parameter) };
            base.OnNavigatedTo(e);
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
        }
    }
}
