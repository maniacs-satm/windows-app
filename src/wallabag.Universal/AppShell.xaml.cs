using System;
using wallabag.Common;
using wallabag.ViewModels;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace wallabag.Universal
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
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
                CheckTogglePaneButtonSizeChanged();

                if (AppSettings.Instance.HamburgerPositionIsRight)
                {
                    HamburgerToggleButton.HorizontalAlignment = HorizontalAlignment.Right;
                    splitView.PanePlacement = SplitViewPanePlacement.Right;
                }
            };

            splitView.RegisterPropertyChangedCallback(SplitView.DisplayModeProperty, (sender, args) =>
            {
                CheckTogglePaneButtonSizeChanged();
            });

            SystemNavigationManager.GetForCurrentView().BackRequested += AppShell_BackRequested;
        }

        #region BackRequested Handlers

        private void AppShell_BackRequested(object sender, BackRequestedEventArgs e)
        {
            bool handled = e.Handled;
            BackRequested(ref handled);
            e.Handled = handled;
        }

        private void BackRequested(ref bool handled)
        {
            // Get a hold of the current frame so that we can inspect the app back stack.
            if (AppFrame == null)
                return;

            // Check to see if this is the top-most page on the app back stack.
            if (AppFrame.CanGoBack && !handled)
            {
                // If not, set the event to handled and go back to the previous page in the app.
                handled = true;
                AppFrame.GoBack();
            }
        }

        #endregion

        #region Navigation

        private StateTriggerBase _defaultStateTrigger;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            frame.Navigate(typeof(Views.ContentPage));
        }

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
                    splitView.IsPaneOpen = false;
                    splitView.DisplayMode = SplitViewDisplayMode.Overlay;
                    HamburgerToggleButton.Visibility = Visibility.Collapsed;
                    _defaultStateTrigger = DefaultState.StateTriggers[0];
                    DefaultState.StateTriggers.Clear();
                }
                else
                {
                    HamburgerToggleButton.Visibility = Visibility.Visible;
                    if (DefaultState.StateTriggers.Count == 0)
                        DefaultState.StateTriggers.Add(_defaultStateTrigger);
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

        #region Hamburger button

        public Rect TogglePaneButtonRect { get; private set; }
        public event TypedEventHandler<AppShell, Rect> TogglePaneButtonRectChanged;

        /// <summary>
        /// Check for the conditions where the navigation pane does not occupy the space under the floating 
        /// hamburger button and trigger the event.
        /// </summary>
        private void CheckTogglePaneButtonSizeChanged()
        {
            if (splitView.DisplayMode == SplitViewDisplayMode.Inline ||
                splitView.DisplayMode == SplitViewDisplayMode.Overlay)
            {
                var transform = HamburgerToggleButton.TransformToVisual(this);
                var rect = transform.TransformBounds(new Rect(0, 0, HamburgerToggleButton.ActualWidth, HamburgerToggleButton.ActualHeight));
                TogglePaneButtonRect = rect;
            }
            else
            {
                TogglePaneButtonRect = new Rect();
            }

            var handler = TogglePaneButtonRectChanged;
            if (handler != null)
            {
                handler(this, TogglePaneButtonRect);
            }
        }

        private void HamburgerToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            CheckTogglePaneButtonSizeChanged();
        }

        #endregion

        private async void AddItemButton_Click(object sender, RoutedEventArgs e)
        {
            await AddItemContentDialog.ShowAsync();
        }
    }
}
