using wallabag.DataModel;
using wallabag.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace wallabag.Views
{
    public sealed partial class ContentPage : Page
    {
        public MainViewModel ViewModel
        {
            get { return (MainViewModel)DataContext; }
            set { DataContext = value; }
        }

        public ContentPage()
        {
            InitializeComponent();
        }

        private void ItemGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var clickedItem = (ItemViewModel)e.ClickedItem;
            ViewModel.CurrentItem = clickedItem;

            Frame.Navigate(typeof(SingleItemPage), clickedItem.Model.Id, new DrillInNavigationTransitionInfo());
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (ViewModel == null)
                ViewModel = new MainViewModel();

            if (e.Parameter != null && e.Parameter.GetType() == typeof(int))
            {
                var id = (int)e.Parameter;
                ViewModel.CurrentItem = new ItemViewModel(await DataSource.GetItemAsync(id));
            }

            if (ViewModel.CurrentItemIsNotNull)
                ItemGridView.SelectedItem = ViewModel.CurrentItem;

            ViewModel.RefreshCommand.Execute(0);
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            ItemGridView.SelectedItem = ViewModel.CurrentItem;
        }
    }
}
