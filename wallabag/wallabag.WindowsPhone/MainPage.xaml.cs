using System;
using wallabag.Common;
using wallabag.DataModel;
using wallabag.Views;
using Windows.UI.Xaml.Controls;

namespace wallabag
{
    public sealed partial class MainPage : basicPage
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void SaveState(SaveStateEventArgs e)
        {
            // Save the selected pivot item via the index.
            e.PageState.Add("SelectedPivotItem", mainPivot.SelectedIndex);
            base.SaveState(e);
        }

        protected override void LoadState(LoadStateEventArgs e)
        {
            if (e.PageState != null)
            {
                // Load the selected pivot item via the index.
                if (e.PageState.ContainsKey("SelectedPivotItem"))
                    mainPivot.SelectedIndex = (int)e.PageState["SelectedPivotItem"];
            }
            base.LoadState(e);
        }

        private async void AppBarButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await new AddLink().ShowAsync(); // not longer used. Coming back in wallabag v2.
        }

        private void AppBarButton_Click_1(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SettingsPage)); // Open the settings page.
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // If the user is pressing a button, then navigate to the ItemPage with the pressed item as parameter.
            this.Frame.Navigate(typeof(ItemPage), ((Item)e.ClickedItem).UniqueId);
        }

    }
}
