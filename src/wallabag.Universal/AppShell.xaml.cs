using System;
using wallabag.Common;
using wallabag.Models;
using wallabag.Services;
using wallabag.ViewModels;
using Windows.ApplicationModel.DataTransfer;
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

            Loaded += (sender, args) => { Current = this; };
        }

        #region Navigation

        private StateTriggerBase[] _adaptiveTrigger = new StateTriggerBase[2];
        private void OnNavigatedToPage(object sender, NavigationEventArgs e)
        {
            // After a successful navigation set keyboard focus to the loaded page
            if (e.Content is Page && e.Content != null)
            {
                var control = (Page)e.Content;
                control.Loaded += Page_Loaded;

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

        private bool AppFrameContentIsNotFirstStartPage
        {
            get { return (AppFrame.Content.GetType() != typeof(Views.FirstStartPage)); }
        }
        

        private async void Grid_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.WebLink) && AppFrameContentIsNotFirstStartPage)
            {
                HideDragDropGridStoryboard.Begin();

                var item = await e.DataView.GetWebLinkAsync();
                await DataService.AddItemAsync(item.ToString());
            }
        }

        private void Grid_DragLeave(object sender, DragEventArgs e) { HideDragDropGridStoryboard.Begin(); }

        private void Grid_DragEnter(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.WebLink) && AppFrameContentIsNotFirstStartPage)
            {
                ShowDragDropGridStoryboard.Begin();
                e.AcceptedOperation = DataPackageOperation.Move;
            }
        }
    }
}
