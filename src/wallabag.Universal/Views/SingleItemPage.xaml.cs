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
            WebView.NavigationStarting += WebView_NavigationStarting;
            //this.Loaded += (s, e) =>
            //{
            //    MarkAsReadButton_Click(this, new RoutedEventArgs());
            //    MarkAsFavoriteButton_Click(this, new RoutedEventArgs());
            //};
        }

        private async void WebView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            if (args.Uri != null)
            {
                args.Cancel = true;
                await Windows.System.Launcher.LaunchUriAsync(args.Uri);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) { dataTransferManager.DataRequested -= SingleItemPage_DataRequested; }

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
                    data.SetWebLink(new Uri(ViewModel.CurrentItem.Model.Url));
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
            parameters.Add(AppSettings.TextAlignment);

            if (ViewModel.CurrentItem != null)
                await WebView.InvokeScriptAsync("changeHtmlAttributes", parameters);
        }

        private void WebView_ScriptNotify(object sender, NotifyEventArgs e)
        {
            ViewModel.CurrentItem.Model.ReadingProgress = e.Value;
        }

        private async void FontFamilyButton_Click(object sender, RoutedEventArgs e)
        {
            if (AppSettings.FontFamily == "serif")
                AppSettings.FontFamily = "sans";
            else
                AppSettings.FontFamily = "serif";

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

        private async void IncreaseFontSize(object sender, RoutedEventArgs e) { ViewModel.FontSize += 1; await ChangeHtmlAttributesAsync(); }
        private async void DecreaseFontSize(object sender, RoutedEventArgs e) { ViewModel.FontSize -= 1; await ChangeHtmlAttributesAsync(); }

        private void MarkAsReadButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.MarkItemAsReadCommand.Execute(null);

            if (ViewModel.CurrentItem.Model.IsRead)
                MarkAsReadButton.Icon = new SymbolIcon(Symbol.Add);
            else
                MarkAsReadButton.Icon = new SymbolIcon(Symbol.Accept);
        }

        private void MarkAsFavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CurrentItem.SwitchFavoriteStatusCommand.Execute(null);

            if (ViewModel.CurrentItem.Model.IsStarred)
                MarkAsFavoriteButton.Icon = new SymbolIcon(Symbol.UnFavorite);
            else
                MarkAsFavoriteButton.Icon = new SymbolIcon(Symbol.Favorite);
        }

        private PathIcon TextAlignJustifyPathIcon;
        private async void ChangeTextAlignButton_Click(object sender, RoutedEventArgs e)
        {
            if (TextAlignJustifyPathIcon == null)
                TextAlignJustifyPathIcon = (sender as Button).Content as PathIcon;

            if (AppSettings.TextAlignment == "left")
            {
                AppSettings.TextAlignment = "justify";
                ChangeTextAlignButton.Content = "";
            }
            else
            {
                AppSettings.TextAlignment = "left";
                ChangeTextAlignButton.Content = TextAlignJustifyPathIcon;
            }
            await ChangeHtmlAttributesAsync();
        }
    }
}
