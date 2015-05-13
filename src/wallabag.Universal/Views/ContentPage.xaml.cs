using wallabag.DataModel;
using Windows.UI.Xaml.Controls;

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
            this.InitializeComponent();
        }

        private void ItemListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            WebView.NavigateToString((e.ClickedItem as ItemViewModel).ContentWithHeader);
        }
    }
}
