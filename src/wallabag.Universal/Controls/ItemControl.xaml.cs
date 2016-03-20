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

        public ItemControl()
        {
            InitializeComponent();          
            PointerEntered += BottomGrid_PointerEntered;
            PointerExited += BottomGrid_PointerExited;

            DataContextChanged += ItemControl_DataContextChanged;
        }

        private void ItemControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue is ItemViewModel && string.IsNullOrEmpty(ViewModel.Model.PreviewPictureUri))
            {
                var newBackground = new SolidColorBrush((Color)Resources["SystemAccentColor"]);
                newBackground.Opacity = 0.5;
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
        private void BottomGrid_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            _PointerExited = false;

            if (!_PointerExited && WindowWidth >= 720)
                ShowOverlayStoryboard.Begin();
        }
    }
}
