using System.Threading.Tasks;
using wallabag.Common;
using wallabag.ViewModels;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace wallabag.Controls
{
    public sealed partial class ItemControl : UserControl
    {
        private double WindowWidth { get { return Window.Current.CoreWindow.Bounds.Width; } }

        public ItemControl()
        {
            InitializeComponent();
            if (!Helpers.IsPhone)
                ContextMenuGrid.PointerExited += (s, e) => HideTouchMenu();
            ContextMenuGrid.PointerWheelChanged += (s, e) => HideTouchMenu();
            this.PointerEntered += BottomGrid_PointerEntered;
            this.PointerExited += BottomGrid_PointerExited;
            this.Loaded += ItemControl_Loaded;

            foreach (AppBarButton button in stackPanel.Children)
                button.Click += (s, e) => HideTouchMenu();
        }

        private void ItemControl_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var itemViewModel = DataContext as ItemViewModel;
            if (itemViewModel != null && string.IsNullOrEmpty(itemViewModel.Model.PreviewPictureUri))
            {
                var newBackground = new SolidColorBrush((Color)Resources["SystemAccentColor"]);
                newBackground.Opacity = 0.3;
                RootGrid.Background = newBackground;
            }
        }

        private bool _PointerExited;
        private void BottomGrid_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            _PointerExited = true;
            if (WindowWidth >= 720)
                HideOverlayStoryboard.Begin();
        }
        private async void BottomGrid_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            _PointerExited = false;
            await Task.Delay(666);

            if (!_PointerExited && WindowWidth >= 720)
                ShowOverlayStoryboard.Begin();
        }

        private void HideTouchMenu() =>
            (ContextMenuGrid.Resources["HideContextMenu"] as Storyboard).Begin();

    }
}
