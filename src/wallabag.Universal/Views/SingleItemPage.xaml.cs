using PropertyChanged;
using wallabag.DataModel;
using wallabag.ViewModels;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace wallabag.Views
{
    [ImplementPropertyChanged]
    public sealed partial class SingleItemPage : Common.basicPage
    {
        private DataTransferManager dataTransferManager;
        public SingleItemPageViewModel ViewModel { get; set; }

        public SingleItemPage()
        {
            InitializeComponent();

            dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += SingleItemPage_DataRequested;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter != null && e.Parameter.GetType() == typeof(int))
            {
                ViewModel = new SingleItemPageViewModel() { CurrentItem = new ItemViewModel(await DataSource.GetItemAsync((int)e.Parameter)) };
                await ViewModel.CurrentItem.CreateContentFromTemplate();

                ApplicationView.GetForCurrentView().Title = ViewModel.CurrentItem.Model.Title;
                WebView.NavigateToString(ViewModel.CurrentItem.ContentWithHeader);
            }

            var backStack = Frame.BackStack;
            var backStackCount = backStack.Count;

            if (backStackCount > 0)
            {
                var masterPageEntry = backStack[backStackCount - 1];
                backStack.RemoveAt(backStackCount - 1);

                // Doctor the navigation parameter for the master page so it
                // will show the correct item in the side-by-side view.
                var modifiedEntry = new PageStackEntry(
                    masterPageEntry.SourcePageType,
                    ViewModel.CurrentItem.Model.Id,
                    masterPageEntry.NavigationTransitionInfo
                    );
                backStack.Add(modifiedEntry);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            ApplicationView.GetForCurrentView().Title = string.Empty;
        }

        void NavigateBackForWideState(bool useTransition)
        {
            // Temporary disabled.
            //if (useTransition)
            //    Frame.GoBack(new EntranceNavigationTransitionInfo());
            //else
            //    Frame.GoBack(new SuppressNavigationTransitionInfo());
        }

        private bool ShouldGoToWideState() { return Window.Current.Bounds.Width >= 1200; }

        private void PageRoot_Loaded(object sender, RoutedEventArgs e)
        {
            if (ShouldGoToWideState())
            {
                // We shouldn't see this page since we are in "wide master-detail" mode.
                // Play a transition as we are navigating from a separate page.
                NavigateBackForWideState(useTransition: true);
            }
            else
            {
                // Realize the main page content.
                FindName("RootPanel");
            }

            Window.Current.SizeChanged += Window_SizeChanged; ;
        }

        private void Window_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            if (ShouldGoToWideState())
            {
                // Make sure we are no longer listening to window change events.
                Window.Current.SizeChanged -= Window_SizeChanged;

                // We shouldn't see this page since we are in "wide master-detail" mode.
                NavigateBackForWideState(useTransition: false);
            }
        }

        private void PageRoot_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= Window_SizeChanged;
        }

        #region Data transfer
        private bool shareContent = false;
        private void SingleItemPage_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var data = args.Request.Data;
            if (shareContent)
                data.SetHtmlFormat(ViewModel.CurrentItem.ContentWithHeader);
            else
                data.SetWebLink(new System.Uri(ViewModel.CurrentItem.Model.Url));
            data.Properties.Title = ViewModel.CurrentItem.Model.Title;
        }

        private void ShareLinkFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            shareContent = false;
            DataTransferManager.ShowShareUI();
        }
        private void ShareContentFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            shareContent = true;
            DataTransferManager.ShowShareUI();
        }
        #endregion

        private void CloseFlyoutButton_Click(object sender, RoutedEventArgs e) { TagFlyout.Hide(); }
    }
}
