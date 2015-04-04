using wallabag.Common;
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
using wallabag.ViewModels;

// Die Elementvorlage "Standardseite" ist unter http://go.microsoft.com/fwlink/?LinkId=234237 dokumentiert.

namespace wallabag.Views
{
    /// <summary>
    /// Eine Standardseite mit Eigenschaften, die die meisten Anwendungen aufweisen.
    /// </summary>
    public sealed partial class SingleItemPage : basicPage
    {
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            DataContext = new SingleItemPageViewModel((int)e.Parameter);
            base.OnNavigatedTo(e);
        }
    }
}
