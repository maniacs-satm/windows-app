﻿using System;
using wallabag.Common;
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
                }
            };
        }

        #region Navigation

        private StateTriggerBase _defaultStateTrigger;
        private SplitViewDisplayMode _lastSplitViewDisplayMode = SplitViewDisplayMode.CompactInline;
        private bool _isPaneOpen = false;

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
                    _defaultStateTrigger = DefaultState.StateTriggers[0];
                    _lastSplitViewDisplayMode = splitView.DisplayMode;
                    _isPaneOpen = splitView.IsPaneOpen;

                    splitView.IsPaneOpen = false;
                    splitView.DisplayMode = SplitViewDisplayMode.Overlay;
                    HamburgerToggleButton.Visibility = Visibility.Collapsed;
                    DefaultState.StateTriggers.Clear();
                }
                else
                {
                    HamburgerToggleButton.Visibility = Visibility.Visible;
                    if (DefaultState.StateTriggers.Count == 0)
                        DefaultState.StateTriggers.Add(_defaultStateTrigger);

                    splitView.DisplayMode = _lastSplitViewDisplayMode;
                    splitView.IsPaneOpen = _isPaneOpen;
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
       
        private async void AddItemButton_Click(object sender, RoutedEventArgs e)
        {
            await AddItemContentDialog.ShowAsync();
        }
    }
}
