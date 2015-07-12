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
        public SingleItemPageViewModel ViewModel { get { return (SingleItemPageViewModel)DataContext; } }

        public SingleItemPage()
        {
            InitializeComponent();

            dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += SingleItemPage_DataRequested;
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

        private async Task ExecuteJavaScript()
        {
            List<string> parameters = new List<string>();
            parameters.Add(AppSettings.Instance.ColorScheme);
            parameters.Add(AppSettings.Instance.FontFamily);
            parameters.Add(AppSettings.Instance.FontSize.ToString());
            parameters.Add(AppSettings.Instance.LineHeight.ToString());

            if (ViewModel.CurrentItem != null)
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
