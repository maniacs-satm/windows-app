using wallabag.Common;
using wallabag.DataModel;
using wallabag.Views;
using Windows.UI.ApplicationSettings;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;

namespace wallabag
{
    public sealed partial class MainPage : basicPage
    {
        public MainPage()
        {
            this.InitializeComponent();
            
            // To make the wallabag settings available from the charms bar, it is required to handle this event.
            SettingsPane.GetForCurrentView().CommandsRequested += MainPage_CommandsRequested;
        }

        protected override void SaveState(SaveStateEventArgs e)
        {
            // These PageState settings are saving the selected menu item.
            e.PageState.Add("unreadItemsActive", unreadItemsMenuButton.IsChecked);
            e.PageState.Add("favouriteItemsActive", favouriteItemsMenuButton.IsChecked);
            e.PageState.Add("archivedItemsActive", archivedItemsMenuButton.IsChecked);
            base.SaveState(e);
        }

        protected override void LoadState(LoadStateEventArgs e)
        {
            // if PageState is not nothing, then...
            if (e.PageState != null)
            {
                // ..load the Checked property of each button.
                if (e.PageState.ContainsKey("unreadItemsActive"))
                    unreadItemsMenuButton.IsChecked = (bool)e.PageState["unreadItemsActive"];
                if (e.PageState.ContainsKey("favouriteItemsActive"))
                    favouriteItemsMenuButton.IsChecked = (bool)e.PageState["favouriteItemsActive"];
                if (e.PageState.ContainsKey("archivedItemsActive"))
                    archivedItemsMenuButton.IsChecked = (bool)e.PageState["archivedItemsActive"];
            }
            base.LoadState(e);
        }

        protected override void ChangedSize(double width, double height)
        {
            // If the app width is 500px or greater (portrait and landscape)
            if (width >= 500)
            {
                unreadItemsText.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                favouriteItemsText.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                archivedItemsText.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                layoutRoot.ColumnDefinitions.Clear();
                layoutRoot.RowDefinitions.Clear();
                layoutRoot.RowDefinitions.Add(new RowDefinition() { Height = new Windows.UI.Xaml.GridLength(100) });
                layoutRoot.RowDefinitions.Add(new RowDefinition() { Height = new Windows.UI.Xaml.GridLength(1, Windows.UI.Xaml.GridUnitType.Star) });

                navigationGrid.RowDefinitions.Clear();
                navigationGrid.ColumnDefinitions.Clear();
                navigationGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new Windows.UI.Xaml.GridLength(100) });
                navigationGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new Windows.UI.Xaml.GridLength(1, Windows.UI.Xaml.GridUnitType.Star) });

                Grid.SetColumn(menuStackPanel, 1);
                menuStackPanel.Orientation = Orientation.Horizontal;

                Grid.SetRow(unreadItems, 1);
                Grid.SetRow(favouriteItems, 1);
                Grid.SetRow(archivedItems, 1);
            }
            if (width >= 800 || ApplicationView.GetForCurrentView().Orientation == ApplicationViewOrientation.Portrait)
            {
                unreadItemsText.Visibility = Windows.UI.Xaml.Visibility.Visible;
                favouriteItemsText.Visibility = Windows.UI.Xaml.Visibility.Visible;
                archivedItemsText.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            if (width >= 1100 && ApplicationView.GetForCurrentView().Orientation == ApplicationViewOrientation.Landscape)
            {
                layoutRoot.RowDefinitions.Clear();
                layoutRoot.ColumnDefinitions.Clear();
                layoutRoot.ColumnDefinitions.Add(new ColumnDefinition() { Width = new Windows.UI.Xaml.GridLength(320) });
                layoutRoot.ColumnDefinitions.Add(new ColumnDefinition() { Width = new Windows.UI.Xaml.GridLength(1, Windows.UI.Xaml.GridUnitType.Star) });

                navigationGrid.ColumnDefinitions.Clear();
                navigationGrid.RowDefinitions.Clear();
                navigationGrid.RowDefinitions.Add(new RowDefinition() { Height = new Windows.UI.Xaml.GridLength(180) });
                navigationGrid.RowDefinitions.Add(new RowDefinition() { Height = new Windows.UI.Xaml.GridLength(1, Windows.UI.Xaml.GridUnitType.Star) });

                Grid.SetColumn(menuStackPanel, 0);
                menuStackPanel.Orientation = Orientation.Vertical;

                Grid.SetRow(unreadItems, 0);
                Grid.SetRow(favouriteItems, 0);
                Grid.SetRow(archivedItems, 0);
            }
        }

        void MainPage_CommandsRequested(Windows.UI.ApplicationSettings.SettingsPane sender, Windows.UI.ApplicationSettings.SettingsPaneCommandsRequestedEventArgs args)
        {
            // The following command adds an "General" button to the Settings pane.
            SettingsCommand generalSettings = new SettingsCommand("generalSettings", Helpers.LocalizedString("generalSettingsText"), (handler) =>
            {
                generalSettingsFlyout settingsFlyout = new generalSettingsFlyout();
                settingsFlyout.Show();
            });

            // And this command adds an "Reading" button to the Settings pane.
            SettingsCommand readingSettings = new SettingsCommand("readingSettings", Helpers.LocalizedString("readingSettingsText"), (handler) =>
            {
                readingSettingsFlyout readingFlyout = new readingSettingsFlyout();
                readingFlyout.Show();
            });

            // To avoid duplicate items, it is required to perform this step.
            if (args.Request.ApplicationCommands.Count == 0)
            {
                args.Request.ApplicationCommands.Add(generalSettings);
                args.Request.ApplicationCommands.Add(readingSettings);
            }
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // If the user is pressing a button, then navigate to the ItemPage with the pressed item as parameter.
            this.Frame.Navigate(typeof(ItemPage), ((Item)e.ClickedItem).UniqueId);
        }
    }
}
