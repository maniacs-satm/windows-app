using wallabag.Common;
using wallabag.Models;
using wallabag.ViewModels;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace wallabag.Universal
{
    public sealed partial class AppShell : Page
    {
        public MainViewModel ViewModel { get { return (MainViewModel)DataContext; } }
        public Frame AppFrame { get { return frame; } }
        public static AppShell Current = null;

        public AppShell()
        {
            InitializeComponent();

            Loaded += (sender, args) =>
            {
                Current = this;
                HamburgerToggleButton.Focus(FocusState.Programmatic);

                if (AppSettings.HamburgerPositionIsRight)
                {
                    HamburgerToggleButton.HorizontalAlignment = HorizontalAlignment.Right;
                    splitView.PanePlacement = SplitViewPanePlacement.Right;
                    frame.Margin = new Thickness(0, 0, 48, 0);
                }
            };
        }

        #region Navigation

        private bool _isPaneOpen = false;
        private StateTriggerBase[] _adaptiveTrigger = new StateTriggerBase[2];
        private void OnNavigatedToPage(object sender, NavigationEventArgs e)
        {
            // After a successful navigation set keyboard focus to the loaded page
            if (e.Content is Page && e.Content != null)
            {
                var control = (Page)e.Content;
                control.Loaded += Page_Loaded;

                // If navigating to the SingleItemPage, hide the menu.
                if (e.Content.GetType() == typeof(Views.SingleItemPage))
                {
                    // Save the current values in the variables before overriding them
                    _isPaneOpen = splitView.IsPaneOpen;
                    _adaptiveTrigger[0] = NarrowState.StateTriggers[0];
                    _adaptiveTrigger[1] = WideState.StateTriggers[0];

                    NarrowState.StateTriggers.Clear();
                    WideState.StateTriggers.Clear();

                    splitView.IsPaneOpen = false;
                    splitView.DisplayMode = SplitViewDisplayMode.Overlay;
                    frame.Margin = new Thickness(0);
                    HamburgerToggleButton.Visibility = Visibility.Collapsed;
                }
                else
                {
                    HamburgerToggleButton.Visibility = Visibility.Visible;
                    if (AppSettings.HamburgerPositionIsRight)
                        frame.Margin = new Thickness(0, 0, 48, 0);
                    else
                        frame.Margin = new Thickness(48, 0, 0, 0);


                    splitView.IsPaneOpen = _isPaneOpen;
                    if (Window.Current.Bounds.Width >= 720)
                    {
                        splitView.DisplayMode = SplitViewDisplayMode.CompactOverlay;

                        if (AppSettings.HamburgerPositionIsRight)
                            frame.Margin = new Thickness(0, 0, 48, 0);
                        else
                            frame.Margin = new Thickness(48, 0, 0, 0);
                    }
                    else
                    {
                        splitView.DisplayMode = SplitViewDisplayMode.Overlay;
                        frame.Margin = new Thickness(0);
                    }

                    if (_adaptiveTrigger[0] != null)
                    {
                        NarrowState.StateTriggers.Add(_adaptiveTrigger[0]);
                        WideState.StateTriggers.Add(_adaptiveTrigger[1]);
                    }
                }

                if (AppFrame != null && AppFrame.CanGoBack)
                    SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                else
                    SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ((Page)sender).Focus(FocusState.Programmatic);
            ((Page)sender).Loaded -= Page_Loaded;
        }

        #endregion

        private void ShowTagsMenuButton_Click(object sender, RoutedEventArgs e)
        {
            splitView.IsPaneOpen = true;
        }

        private async void TagsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            await ViewModel.LoadItemsAsync(new Services.FilterProperties() { ItemType = Services.FilterProperties.FilterPropertiesItemType.All, FilterTag = (Tag)e.ClickedItem });
            splitView.IsPaneOpen = false;
        }
    }
}
