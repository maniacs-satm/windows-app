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
using Windows.UI.Xaml.Navigation;

namespace wallabag.Views
{
    [ImplementPropertyChanged]
    public sealed partial class ContentPage : Page
    {
        public MainViewModel ViewModel { get { return (MainViewModel)DataContext; } }

        public ContentPage()
        {
            InitializeComponent();
            ShowSearchStoryboard.Completed += (s, e) => { SearchQueryAutoSuggestBox.Focus(FocusState.Programmatic); };
            HideSearchStoryboard.Completed += (s, e) => { ItemGridView.Focus(FocusState.Programmatic); };
            ContentSplitView.PaneClosing += (s, e) =>
            {
                Messenger.Default.Send(new NotificationMessage("FilterView"));
                HideFilterStoryboard.Begin();
                _IsFilterVisible = false;
            };
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Messenger.Default.Register<NotificationMessage>(this, message =>
            {
                if (message.Notification == "HideSearch")
                    searchToggleButton_Click(this, new RoutedEventArgs());
                else if (message.Notification == "HideOverlay")
                    ContentSplitView.IsPaneOpen = false;
                else if (message.Notification == "FinishMultipleSelection")
                    ItemGridView.SelectionMode = ListViewSelectionMode.None;
            });
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e) => Messenger.Default.Unregister(this);

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

                var itemsToCancel = VisualTreeHelper.FindElementsInHostCoordinates(e.GetPosition(null), ItemGridView);
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

            ItemGridView.SelectionMode = ListViewSelectionMode.Multiple;
        }
        private void DisableMultipleSelection(object sender, RoutedEventArgs e)
        {
            SetMultipleSelectionEnabledProperty(false);
            SetItemClickEnabledProperty(true);

            ItemGridView.SelectionMode = ListViewSelectionMode.None;
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
            if (_IsSearchVisible == false && (searchToggleButton.Icon as SymbolIcon).Symbol == Symbol.Cancel) //CloseSearchStoryboard
            {
                _IsSearchVisible = false;
                CloseSearchStoryboard.Begin();
                Messenger.Default.Send(new NotificationMessage("ResetSearch"));
            }
            else if (_IsSearchVisible == false) //ShowSearchStoryboard
            {
                _IsSearchVisible = true;
                ShowSearchStoryboard.Begin();
                if (AppSettings.OpenTheFilterPaneWithTheSearch)
                    FilterButton_Click(sender, e);
            }
            else //HideSearchStoryboard
            {
                _IsSearchVisible = false;
                HideSearchStoryboard.Begin();
                if (_IsFilterVisible)
                    FilterButton_Click(sender, e);
            }
        }

        private bool _IsFilterVisible = false;
        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (_IsFilterVisible == false)
            {
                _IsFilterVisible = true;
                ShowFilterStoryboard.Begin();
            }
            else
            {
                _IsFilterVisible = false;
                HideFilterStoryboard.Begin();
            }
        }

        private void OverlayGrid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (_IsSearchVisible)
            {
                HideSearchStoryboard.Begin();
                if (string.IsNullOrWhiteSpace(SearchQueryAutoSuggestBox.Text))
                    CloseSearchStoryboard.Begin();
                _IsSearchVisible = false;
            }
            if (_IsFilterVisible)
            {
                HideFilterStoryboard.Begin();
                _IsFilterVisible = false;
            }
        }

        private void CloseSearchButton_Click(object sender, RoutedEventArgs e)
        {
            HideSearchStoryboard.Begin();
            if (AppSettings.OpenTheFilterPaneWithTheSearch || !_IsFilterVisible)
            {
                HideFilterStoryboard.Begin();
                _IsFilterVisible = false;
            }
            _IsSearchVisible = false;
        }

        private void ResetFilterButton_Click(object sender, RoutedEventArgs e) { ContentSplitView.IsPaneOpen = false; }

        private void SearchQueryAutoSuggestBox_LostFocus(object sender, RoutedEventArgs e)
        {
            HideSearchStoryboard.Begin();
            CloseSearchStoryboard.Begin();
            _IsSearchVisible = false;
        }
    }
}
