using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace wallabag.Controls
{
    public sealed partial class ItemControl : UserControl
    {
        public ItemControl()
        {
            InitializeComponent();
            ContextMenuGrid.PointerExited += (s, e) => HideTouchMenu();
            ContextMenuGrid.PointerWheelChanged += (s, e) => HideTouchMenu();

            foreach (AppBarButton button in stackPanel.Children)
                button.Click += (s, e) => HideTouchMenu();
        }

        private void HideTouchMenu() =>
            (ContextMenuGrid.Resources["HideContextMenu"] as Storyboard).Begin();

    }
}
