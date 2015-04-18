using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace wallabag.Universal
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void ItemsMenuButton_Click(object sender, RoutedEventArgs e)
        {
            HeaderTextBlock.Text = "Items";
            itemGrid.Visibility = Visibility.Visible; // ensure that the itemGrid is visible, even if the FavoriteMenuButton is selected
            BottomAppBar.Visibility = Visibility.Visible;
        }

        private void TagsMenuButton_Click(object sender, RoutedEventArgs e)
        {
            HeaderTextBlock.Text = "Tags";
            BottomAppBar.Visibility = Visibility.Visible;
        }

        private void SettingsMenuButton_Click(object sender, RoutedEventArgs e)
        {
            HeaderTextBlock.Text = "Settings";
            BottomAppBar.Visibility = Visibility.Collapsed;
        }
    }
}
