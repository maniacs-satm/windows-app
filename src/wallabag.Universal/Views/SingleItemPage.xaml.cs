using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PropertyChanged;
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
            parameters.Add(ViewModel.AppSettings.ColorScheme);
            parameters.Add(ViewModel.AppSettings.FontFamily);
            parameters.Add(ViewModel.AppSettings.FontSize.ToString());
            parameters.Add(ViewModel.AppSettings.LineHeight.ToString());

            await WebView.InvokeScriptAsync("changeHtmlAttributes", parameters);
        }

        private async void Slider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (ViewModel != null)
            {
                if (sender == fontSizeSlider)
                    ViewModel.AppSettings.FontSize = e.NewValue;
                else
                    ViewModel.AppSettings.LineHeight = e.NewValue;
                await ExecuteJavaScript();
            }
        }

        private async void FontFamilyButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender == sansFamilyButton)
                ViewModel.AppSettings.FontFamily = "sans";
            else
                ViewModel.AppSettings.FontFamily = "serif";
            await ExecuteJavaScript();
        }

        private async void ChangeColorSchemeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender == lightColorSchemeButton)
                ViewModel.AppSettings.ColorScheme = "light";
            else if (sender == sepiaColorSchemeButton)
                ViewModel.AppSettings.ColorScheme = "sepia";
            else
                ViewModel.AppSettings.ColorScheme = "dark";
            await ExecuteJavaScript();
        }
    }
}
