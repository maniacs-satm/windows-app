using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PropertyChanged;
using wallabag.Common;
using wallabag.DataModel;
using wallabag.ViewModels;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace wallabag.Views
{
    [ImplementPropertyChanged]
    public sealed partial class SingleItemPage : Page
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
                WebView.NavigationCompleted += async (sender, args) => { await ExecuteJavaScript(); };
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
            dataTransferManager.DataRequested -= SingleItemPage_DataRequested;
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

        private async Task ExecuteJavaScript()
        {
            List<string> parameters = new List<string>();
            parameters.Add(AppSettings.Instance.ColorScheme);
            parameters.Add(AppSettings.Instance.FontFamily);
            parameters.Add(AppSettings.Instance.FontSize.ToString());
            parameters.Add(AppSettings.Instance.LineHeight.ToString());

            await WebView.InvokeScriptAsync("changeHtmlAttributes", parameters);
        }

        private async void Slider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (ViewModel != null)
            {
                if (sender == fontSizeSlider)
                    AppSettings.Instance.FontSize = e.NewValue;
                else
                    AppSettings.Instance.LineHeight = e.NewValue;
                await ExecuteJavaScript();
            }
        }

        private async void FontFamilyButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender == sansFamilyButton)
                AppSettings.Instance.FontFamily = "sans";
            else
                AppSettings.Instance.FontFamily = "serif";
            await ExecuteJavaScript();
        }

        private async void ChangeColorSchemeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender == lightColorSchemeButton)
                AppSettings.Instance.ColorScheme = "light";
            else if (sender == sepiaColorSchemeButton)
                AppSettings.Instance.ColorScheme = "sepia";
            else
                AppSettings.Instance.ColorScheme = "dark";
            await ExecuteJavaScript();
        }
    }
}
