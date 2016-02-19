using GalaSoft.MvvmLight.Messaging;
using PropertyChanged;
using System;
using wallabag.Common;
using wallabag.ViewModels;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace wallabag.Views
{
    [ImplementPropertyChanged]
    public sealed partial class ContentPage : Page
    {
        public MainViewModel ViewModel { get { return (MainViewModel)DataContext; } }
        private GridView _ItemGridView;

        public ContentPage()
        {
            InitializeComponent();
            ShowSearch.Completed += (s, e) => { SearchQueryAutoSuggestBox.Focus(FocusState.Programmatic); };
            HideSearch.Completed += (s, e) => { SearchQueryAutoSuggestBox.Text = string.Empty; };
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Messenger.Default.Register<NotificationMessage>(this, message =>
            {
                if (message.Notification == "HideSearch")
                    searchToggleButton_Click(this, new RoutedEventArgs());
                else if (message.Notification == "HideOverlay")
                    OverlayGrid.Visibility = Visibility.Collapsed;
            });
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e) => Messenger.Default.Unregister(this);

        private void ItemGridView_Loaded(object sender, RoutedEventArgs e) => _ItemGridView = sender as GridView;

        #region Context menu
        private bool _IsShiftPressed = false;
        private bool _IsPointerPressed = false;
        private ItemViewModel _LastFocusedItemViewModel;

        protected override void OnKeyDown(KeyRoutedEventArgs e)
        {
            // Handle Shift+F10
            // Handle MenuKey

            if (e.Key == VirtualKey.Shift)
                _IsShiftPressed = true;

            // Shift+F10 or the 'Menu' key next to Right Ctrl on most keyboards
            else if (_IsShiftPressed && e.Key == VirtualKey.F10
                    || e.Key == VirtualKey.Application)
            {
                var FocusedUIElement = FocusManager.GetFocusedElement() as UIElement;
                if (FocusedUIElement is ContentControl)
                    _LastFocusedItemViewModel = ((ContentControl)FocusedUIElement).Content as ItemViewModel;

                ShowContextMenu(FocusedUIElement, new Point(0, 0));
                e.Handled = true;
            }

            base.OnKeyDown(e);
        }
        protected override void OnKeyUp(KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Shift)
                _IsShiftPressed = false;

            base.OnKeyUp(e);
        }
        protected override void OnHolding(HoldingRoutedEventArgs e)
        {
            if (e.HoldingState == Windows.UI.Input.HoldingState.Started)
            {
                _LastFocusedItemViewModel = (e.OriginalSource as FrameworkElement).DataContext as ItemViewModel;
                ShowContextMenu(null, e.GetPosition(null));
                e.Handled = true;

                _IsPointerPressed = false;

                var itemsToCancel = VisualTreeHelper.FindElementsInHostCoordinates(e.GetPosition(null), _ItemGridView);
                foreach (var item in itemsToCancel)
                    item.CancelDirectManipulations();
            }

            base.OnHolding(e);
        }
        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            _IsPointerPressed = true;

            base.OnPointerPressed(e);
        }
        protected override void OnRightTapped(RightTappedRoutedEventArgs e)
        {
            if (_IsPointerPressed)
            {
                _LastFocusedItemViewModel = (e.OriginalSource as FrameworkElement).DataContext as ItemViewModel;
                ShowContextMenu(null, e.GetPosition(null));

                e.Handled = true;
            }

            base.OnRightTapped(e);
        }
        private void ShowContextMenu(UIElement target, Point offset)
        {
            if (_LastFocusedItemViewModel != null && (bool)ViewModel.IsItemClickEnabled)
            {
                var MyFlyout = Resources["ItemContextMenu"] as MenuFlyout;

                foreach (var item in MyFlyout.Items)
                    item.DataContext = _LastFocusedItemViewModel;

                MyFlyout.ShowAt(target, offset);
            }
        }

        private async void ContextMenuMarkAsRead_Click(object sender, RoutedEventArgs e)
        {
            await _LastFocusedItemViewModel.SwitchReadValueAsync();
            UpdateView();
        }
        private async void ContextMenuMarkAsFavorite_Click(object sender, RoutedEventArgs e)
        {
            await _LastFocusedItemViewModel.SwitchFavoriteValueAsync();
            UpdateView();
        }
        private void ContextMenuShareItem_Click(object sender, RoutedEventArgs e)
            => _LastFocusedItemViewModel.ShareCommand.Execute(null);
        private async void ContextMenuOpenInBrowser_Click(object sender, RoutedEventArgs e)
            => await Launcher.LaunchUriAsync(new Uri(_LastFocusedItemViewModel.Model.Url));
        private async void ContextMenuDeleteItem_Click(object sender, RoutedEventArgs e)
        {
            await _LastFocusedItemViewModel.DeleteAsync();
            UpdateView();
        }
        #endregion

        // Used as shortcuts for the Messenger
        private void UpdateView()
        {
            Messenger.Default.Send(new NotificationMessage("UpdateView"));
        }
        private void SetItemClickEnabledProperty(bool newValue)
        {
            Messenger.Default.Send(new NotificationMessage<bool>(newValue, "SetItemClickEnabled"));
        }
        private void SetMultipleSelectionEnabledProperty(bool newValue)
        {
            Messenger.Default.Send(new NotificationMessage<bool>(newValue, "SetMultipleSelectionEnabled"));
        }

        #region Multiple selection
        private void EnableMultipleSelection(object sender, RoutedEventArgs e)
        {
            SetMultipleSelectionEnabledProperty(true);
            SetItemClickEnabledProperty(false);

            _ItemGridView.SelectionMode = ListViewSelectionMode.Multiple;
        }
        private void DisableMultipleSelection(object sender, RoutedEventArgs e)
        {
            SetMultipleSelectionEnabledProperty(false);
            SetItemClickEnabledProperty(true);

            _ItemGridView.SelectionMode = ListViewSelectionMode.None;
        }
        private void ManageTagsMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            (Resources["ShowEditTagsBorder"] as Storyboard).Begin();
        }
        #endregion

        #region Drag & Drop
        private async void Grid_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.WebLink))
            {
                HideDragDropGridStoryboard.Begin();

                var url = await e.DataView.GetWebLinkAsync();
                Messenger.Default.Send(new NotificationMessage<Uri>(url, "addItem"));
            }
        }

        private void Grid_DragLeave(object sender, DragEventArgs e) { HideDragDropGridStoryboard.Begin(); }

        private void Grid_DragEnter(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.WebLink))
            {
                ShowDragDropGridStoryboard.Begin();
                e.AcceptedOperation = DataPackageOperation.Move;
            }
        }
        #endregion

        private bool _IsSearchVisible = false;
        private void searchToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (_IsSearchVisible == false)
            {
                _IsSearchVisible = true;
                ShowSearch.Begin();
                ShowOverlay.Begin();
                if (AppSettings.OpenTheFilterPaneWithTheSearch)
                    FilterButton_Click(sender, e);
                SetItemClickEnabledProperty(false);
            }
            else
            {
                _IsSearchVisible = false;
                HideSearch.Begin();
                if (_IsFilterPopupVisible)
                    FilterButton_Click(sender, e);
                SetItemClickEnabledProperty(true);
            }
        }

        private bool _IsFilterPopupVisible = false;
        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (_IsFilterPopupVisible == false)
            {
                _IsFilterPopupVisible = true;
                ShowOverlay.Begin();
                ShowFilterPopup.Begin();
            }
            else
            {
                _IsFilterPopupVisible = false;
                if (!_IsSearchVisible) HideOverlay.Begin();
                HideFilterPopup.Begin();
            }
        }

        private void OverlayGrid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (_IsSearchVisible)
            {
                HideSearch.Begin();
                _IsSearchVisible = false;
            }
            if (_IsFilterPopupVisible)
            {
                HideFilterPopup.Begin();
                _IsFilterPopupVisible = false;
            }
            HideOverlay.Begin();
        }

        private void CloseSearchButton_Click(object sender, RoutedEventArgs e)
        {
            HideSearch.Begin();
            if (AppSettings.OpenTheFilterPaneWithTheSearch)
            {
                HideOverlay.Begin();
                HideFilterPopup.Begin();
                _IsFilterPopupVisible = false;
            }
            _IsSearchVisible = false;
        }
    }
}
