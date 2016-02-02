using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Template10.Common;
using wallabag.Common;
using wallabag.Models;
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
    public sealed partial class ContentPage : Page
    {
        public MainViewModel ViewModel { get { return (MainViewModel)DataContext; } }

        private GridView _ItemGridView;
        public bool IsMultipleSelectionEnabled { get; set; } = false;
        private bool IsSearchVisible { get; set; } = false;


        public ContentPage()
        {
            InitializeComponent();
            HideDragDropGridStoryboard.Completed += (s, e) =>
            {
                ViewModel.RefreshItemsAsync(); // TODO: Use the messenger!
            };
        }

        // TODO: Use VisualStates.
        //private void ItemGridView_SizeChanged(object sender, SizeChangedEventArgs e)
        //{
        //    var ItemGridView = sender as GridView;
        //    if (e.NewSize.Width < 720)
        //        ItemGridView.ItemsPanel = ListViewTemplate;
        //    else
        //        ItemGridView.ItemsPanel = GridViewTemplate;
        //}

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
            if (_LastFocusedItemViewModel != null && !IsMultipleSelectionEnabled)
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
            await ViewModel.RefreshItemsAsync();
        }
        private async void ContextMenuMarkAsFavorite_Click(object sender, RoutedEventArgs e)
        {
            await _LastFocusedItemViewModel.SwitchFavoriteValueAsync();
            await ViewModel.RefreshItemsAsync();
        }
        private void ContextMenuShareItem_Click(object sender, RoutedEventArgs e)
            => _LastFocusedItemViewModel.ShareCommand.Execute(null);
        private async void ContextMenuOpenInBrowser_Click(object sender, RoutedEventArgs e)
            => await Launcher.LaunchUriAsync(new Uri(_LastFocusedItemViewModel.Model.Url));
        private async void ContextMenuDeleteItem_Click(object sender, RoutedEventArgs e)
        {
            await _LastFocusedItemViewModel.DeleteAsync();
            await ViewModel.RefreshItemsAsync();
        }
        #endregion

        #region Multiple selection

        private async void multipleSelectToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsMultipleSelectionEnabled)
            {
                SQLite.SQLiteAsyncConnection conn = new SQLite.SQLiteAsyncConnection(Helpers.DATABASE_PATH);

                foreach (ItemViewModel item in _ItemGridView.SelectedItems)
                    await conn.UpdateAsync(item.Model);

                await ViewModel.RefreshItemsAsync();

                IsMultipleSelectionEnabled = false;
                _ItemGridView.SelectionMode = ListViewSelectionMode.None;
                _ItemGridView.IsItemClickEnabled = true;
                MultipleSelectionCommandBar.Visibility = Visibility.Collapsed;
                PrimaryCommandBar.Visibility = Visibility.Visible;
            }
            else
            {
                IsMultipleSelectionEnabled = true;
                _ItemGridView.SelectionMode = ListViewSelectionMode.Multiple;
                _ItemGridView.IsItemClickEnabled = false;
                MultipleSelectionCommandBar.Visibility = Visibility.Visible;
                PrimaryCommandBar.Visibility = Visibility.Collapsed;
            }
        }

        private async void MarkItemsAsReadMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (ItemViewModel item in _ItemGridView.SelectedItems)
            {
                item.Model.IsRead = true;
                await ItemViewModel.UpdateSpecificProperty(item.Model.Id, "archive", true);
            }
            multipleSelectToggleButton_Click(sender, e);
        }
        private async void UnmarkItemsAsReadMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (ItemViewModel item in _ItemGridView.SelectedItems)
            {
                item.Model.IsRead = false;
                await ItemViewModel.UpdateSpecificProperty(item.Model.Id, "archive", false);
            }
            multipleSelectToggleButton_Click(sender, e);
        }
        private async void MarkItemsAsFavoriteMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (ItemViewModel item in _ItemGridView.SelectedItems)
            {
                item.Model.IsStarred = true;
                await ItemViewModel.UpdateSpecificProperty(item.Model.Id, "star", true);
            }
            multipleSelectToggleButton_Click(sender, e);
        }
        private async void UnmarkItemsAsFavoriteMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (ItemViewModel item in _ItemGridView.SelectedItems)
            {
                item.Model.IsStarred = true;
                await ItemViewModel.UpdateSpecificProperty(item.Model.Id, "star", false);
            }
            multipleSelectToggleButton_Click(sender, e);
        }
        private void ManageTagsMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            (Resources["ShowEditTagsBorder"] as Storyboard).Begin();
        }
        private async void DeleteMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (ItemViewModel item in _ItemGridView.SelectedItems)
            {
                item.Model.IsDeleted = true;
                await item.DeleteAsync();
            }
            multipleSelectToggleButton_Click(sender, e);
        }
        #endregion

        #region Drag & Drop
        private async void Grid_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.WebLink))
            {
                HideDragDropGridStoryboard.Begin();

                var item = await e.DataView.GetWebLinkAsync();
                // TODO:  await DataService.AddItemAsync(item.ToString());
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
    }
}
