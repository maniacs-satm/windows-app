using GalaSoft.MvvmLight.Messaging;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wallabag.Common;
using wallabag.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace wallabag.Views
{
    [ImplementPropertyChanged]
    public sealed partial class SingleItemPage : Page
    {
        public SingleItemPageViewModel ViewModel { get { return (SingleItemPageViewModel)DataContext; } }

        public SingleItemPage()
        {
            InitializeComponent();

            ViewModel.CommandBarClosedDisplayMode = AppBarClosedDisplayMode.Hidden;
            WebView.ScriptNotify += WebView_ScriptNotify;
            WebView.NavigationStarting += WebView_NavigationStarting;
            WebView.NavigationCompleted += (s, e) => { ShowContentStoryboard.Begin(); };
            ShowContentStoryboard.Completed += (s, e) =>
            {
                if (ViewModel.ErrorHappened == false)
                    ViewModel.CommandBarClosedDisplayMode = AppBarClosedDisplayMode.Minimal;
            };
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Messenger.Default.Register<NotificationMessage>(this, async message =>
            {
                if (message.Notification == "updateHTML")
                    await ChangeHtmlAttributesAsync();
            });
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e) { Messenger.Default.Unregister(this); }

        private async void WebView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            if (args.Uri != null)
            {
                args.Cancel = true;
                await Windows.System.Launcher.LaunchUriAsync(args.Uri);
            }
        }
        private void WebView_ScriptNotify(object sender, NotifyEventArgs e)
        {
            Messenger.Default.Send(new NotificationMessage<string>(e.Value, "readingProgress"));

            if (float.Parse(e.Value) == 100)
                BottomAppBar.IsOpen = true;
        }

        private async Task ChangeHtmlAttributesAsync()
        {
            List<string> parameters = new List<string>();
            parameters.Add(AppSettings.ColorScheme);
            parameters.Add(AppSettings.FontFamily);
            parameters.Add(AppSettings.FontSize.ToString());
            parameters.Add(AppSettings.TextAlignment);

            await WebView.InvokeScriptAsync("changeHtmlAttributes", parameters);
        }

        private void FontSettingsAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            ShowFormattingOptionsGridStoryboard.Begin();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            HideFormattingOptionsGridStoryboard.Begin();
        }
    }
}
