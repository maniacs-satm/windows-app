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
        public Frame AppFrame { get { return this.frame; } }
        public static AppShell Current = null;

        public AppShell()
        {
            this.InitializeComponent();

            this.Loaded += (sender, args) =>
            {
                Current = this;

                this.HamburgerToggleButton.Focus(FocusState.Programmatic);
            };

            this.splitView.RegisterPropertyChangedCallback(SplitView.DisplayModeProperty, (sender, args) =>
            {
                this.CheckTogglePaneButtonSizeChanged();
            });

            SystemNavigationManager.GetForCurrentView().BackRequested += AppShell_BackRequested;
        }

        #region BackRequested Handlers

        private void AppShell_BackRequested(object sender, BackRequestedEventArgs e)
        {
            bool handled = e.Handled;
            this.BackRequested(ref handled);
            e.Handled = handled;
        }

        private void BackRequested(ref bool handled)
        {
            // Get a hold of the current frame so that we can inspect the app back stack.
            if (this.AppFrame == null)
                return;

            // Check to see if this is the top-most page on the app back stack.
            if (this.AppFrame.CanGoBack && !handled)
            {
                // If not, set the event to handled and go back to the previous page in the app.
                handled = true;
                this.AppFrame.GoBack();
            }
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            bool ignored = false;
            this.BackRequested(ref ignored);
        }

        #endregion

        #region Navigation

        private void OnNavigatedToPage(object sender, NavigationEventArgs e)
        {
            // After a successful navigation set keyboard focus to the loaded page
            if (e.Content is Page && e.Content != null)
            {
                var control = (Page)e.Content;
                control.Loaded += Page_Loaded;

                // If navigating to the SingleItemPage, hide the menu.
                if (e.Content.GetType() == typeof(Views.SingleItemPage))
                    splitView.IsPaneOpen = false;

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
            if (this.splitView.DisplayMode == SplitViewDisplayMode.Inline ||
                this.splitView.DisplayMode == SplitViewDisplayMode.Overlay)
            {
                var transform = this.HamburgerToggleButton.TransformToVisual(this);
                var rect = transform.TransformBounds(new Rect(0, 0, this.HamburgerToggleButton.ActualWidth, this.HamburgerToggleButton.ActualHeight));
                this.TogglePaneButtonRect = rect;
            }
            else
            {
                this.TogglePaneButtonRect = new Rect();
            }

            var handler = this.TogglePaneButtonRectChanged;
            if (handler != null)
            {
                handler(this, this.TogglePaneButtonRect);
            }
        }

        private void HamburgerToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            this.CheckTogglePaneButtonSizeChanged();
        }

        #endregion

        private void UnreadItemsMenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (AppFrame.Content.GetType() != typeof(Views.ContentPage))
                AppFrame.Navigate(typeof(Views.ContentPage));
        }

        private void SettingsMenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (AppFrame.Content.GetType() != typeof(Views.SettingsPage))
                AppFrame.Navigate(typeof(Views.SettingsPage));
        }
    }
}
