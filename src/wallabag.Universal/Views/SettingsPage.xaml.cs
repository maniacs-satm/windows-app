using System;
using wallabag.Common;
using wallabag.ViewModels;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace wallabag.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPageViewModel ViewModel { get { return (SettingsPageViewModel)DataContext; } }

        public SettingsPage()
        {
            InitializeComponent();
            versionTextBlock.Text = GetAppVersion();
        }

        public static string GetAppVersion()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            return string.Format($"{version.Major}.{version.Minor}.{version.Build}");
        }

        private async void InfoPageButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var uriString = string.Empty;
            if (sender == OpenChangelogButton)
                uriString = "https://github.com/wallabag/windows-app/blob/v2/CHANGELOG.md"; //TODO: Update when merged into master branch
            else if (sender == OpenWallabagDocumentationButton)
                uriString = "http://doc.wallabag.org/";
            else if (sender == OpenWallabagTwitterAccountButton)
                uriString = "https://twitter.com/wallabagapp";
            else if (sender == OpenDeveloperTwitterAccountButton)
                uriString = "https://twitter.com/jlnostr";
            else if (sender == ContactDeveloperEmailButton)
                uriString = "mailto:wallabag@jlnostr.de";
            else if (sender == CreateIssueOnGithubButton)
                uriString = "https://github.com/wallabag/windows-app/issues/new";
            else if (sender == RateAndReviewButton)
                uriString = "ms-windows-store://review/?ProductId=" + Package.Current.Id.FamilyName;
            else if (sender == ShareAppButton)
            {
                DataTransferManager.GetForCurrentView().DataRequested += (s, args) =>
                {
                    var data = args.Request.Data;

                    data.SetWebLink(new Uri("https://www.microsoft.com/store/apps/9nblggh11646"));
                    data.Properties.Title = Helpers.LocalizedString("ImUsingWallabagAndYou");
                };
                DataTransferManager.ShowShareUI();
                return;
            }
            await Launcher.LaunchUriAsync(new Uri(uriString));
        }

        private void IntervalRadioButton_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if ((sender as RadioButton).Content != null)
                AppSettings.BackgroundTaskInterval = uint.Parse((sender as RadioButton).Content as string);
        }
}
}
