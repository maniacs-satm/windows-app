using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PropertyChanged;
using wallabag.Common;
using wallabag.ViewModels;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace wallabag.Views
{
    [ImplementPropertyChanged]
    public sealed partial class SingleItemPage : Page
    {
        private DataTransferManager dataTransferManager;
        public SingleItemPageViewModel ViewModel { get { return (SingleItemPageViewModel)DataContext; } }

        public SingleItemPage()
        {
            InitializeComponent();

            dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += SingleItemPage_DataRequested;
            WebView.ScriptNotify += WebView_ScriptNotify;

            if (AppSettings.FontFamily == "sans")
                changeFontFamilyButton.FontFamily = new Windows.UI.Xaml.Media.FontFamily("Georgia");
            else
                changeFontFamilyButton.FontFamily = new Windows.UI.Xaml.Media.FontFamily("Segoe UI");
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            dataTransferManager.DataRequested -= SingleItemPage_DataRequested;
        }

        #region Data transfer
        private bool shareContent = false;
        private void SingleItemPage_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            if (ViewModel.CurrentItem != null)
            {
                var data = args.Request.Data;
                if (shareContent)
                    data.SetHtmlFormat(ViewModel.CurrentItem.ContentWithHeader);
                else
                    data.SetWebLink(new System.Uri(ViewModel.CurrentItem.Model.Url));
                data.Properties.Title = ViewModel.CurrentItem.Model.Title;
            }
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

        private async Task ChangeHtmlAttributesAsync()
        {
            List<string> parameters = new List<string>();
            parameters.Add(AppSettings.ColorScheme);
            parameters.Add(AppSettings.FontFamily);
            parameters.Add(AppSettings.FontSize.ToString());
            parameters.Add(AppSettings.LineHeight.ToString());

            if (ViewModel.CurrentItem != null)
                await WebView.InvokeScriptAsync("changeHtmlAttributes", parameters);
        }

        private void WebView_ScriptNotify(object sender, NotifyEventArgs e)
        {
            float progress;
            float.TryParse(e.Value, out progress);
            ViewModel.CurrentItem.Model.ReadingProgress = progress;
            if (progress == 100)
                BottomAppBar.IsOpen = true;
        }

        private async void Slider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (ViewModel != null)
            {                
                await ChangeHtmlAttributesAsync();
            }
        }

        private async void FontFamilyButton_Click(object sender, RoutedEventArgs e)
        {
            if (AppSettings.FontFamily == "serif")
            {
                AppSettings.FontFamily = "sans";
                changeFontFamilyButton.FontFamily = new Windows.UI.Xaml.Media.FontFamily("Georgia");
            }
            else
            {
                AppSettings.FontFamily = "serif";
                changeFontFamilyButton.FontFamily = new Windows.UI.Xaml.Media.FontFamily("Segoe UI");
            }
            await ChangeHtmlAttributesAsync();
        }

        private async void ChangeColorSchemeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender == lightColorSchemeButton)
                AppSettings.ColorScheme = "light";
            else if (sender == sepiaColorSchemeButton)
                AppSettings.ColorScheme = "sepia";
            else if (sender == darkColorSchemeButton)
                AppSettings.ColorScheme = "dark";
            else
                AppSettings.ColorScheme = "black";
            await ChangeHtmlAttributesAsync();
        }
    }
}
