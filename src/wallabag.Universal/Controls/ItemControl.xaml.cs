using wallabag.ViewModels;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace wallabag.Controls
{
    public sealed partial class ItemControl : UserControl
    {
        private double WindowWidth { get { return Window.Current.CoreWindow.Bounds.Width; } }
        public ItemViewModel ViewModel { get { return DataContext as ItemViewModel; } }

        public ItemControl() { InitializeComponent(); }
    }
}
