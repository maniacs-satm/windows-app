using System.Threading.Tasks;
using wallabag.Common;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace wallabag.Controls
{
    public sealed partial class ItemControl : UserControl
    {
        public ItemControl()
        {
            InitializeComponent();
            if (!Helpers.IsPhone)
                ContextMenuGrid.PointerExited += (s, e) => HideTouchMenu();
            ContextMenuGrid.PointerWheelChanged += (s, e) => HideTouchMenu();
            this.PointerEntered += BottomGrid_PointerEntered;
            this.PointerExited += BottomGrid_PointerExited;

            foreach (AppBarButton button in stackPanel.Children)
                button.Click += (s, e) => HideTouchMenu();
        }

        private bool _PointerExited;
        private void BottomGrid_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            _PointerExited = true;
            HideOverlayStoryboard.Begin();
        }
        private async void BottomGrid_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            _PointerExited = false;
            await Task.Delay(300);

            if (!_PointerExited)
                ShowOverlayStoryboard.Begin();
        }
        
        private void HideTouchMenu() =>
            (ContextMenuGrid.Resources["HideContextMenu"] as Storyboard).Begin();

    }
}
