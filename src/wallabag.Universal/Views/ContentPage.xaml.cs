using wallabag.DataModel;
using wallabag.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace wallabag.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ContentPage : Page
    {
        public ContentPage()
        {
            InitializeComponent();
        }

        private void ItemListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            WebView.NavigateToString((e.ClickedItem as ItemViewModel).ContentWithHeader);
            var dc = (MainViewModel)this.DataContext;
            dc.CurrentItem = e.ClickedItem as ItemViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var dc = (MainViewModel)this.DataContext;
            if (dc.CurrentItemIsNotNull)
            {
                WebView.NavigateToString(dc.CurrentItem.ContentWithHeader);
                ItemListView.SelectedItem = dc.CurrentItem;
            }
        }
    }
}
