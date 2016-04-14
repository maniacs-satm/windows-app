using System;
using wallabag.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace wallabag.Controls
{
    public sealed partial class ItemControl : UserControl
    {
        private double WindowWidth { get { return Window.Current.CoreWindow.Bounds.Width; } }
        private double AspectRatio { get; set; } = 1.5;
        public ItemViewModel ViewModel { get { return DataContext as ItemViewModel; } }
        private bool _IsImageLoaded = false;

        public ItemControl()
        {
            InitializeComponent();
            SizeChanged += ItemControl_SizeChanged;

            if (WindowWidth >= 720)
            {
                MetadataStackPanel.RequestedTheme = ElementTheme.Light;
                PreviewText.Visibility = Visibility.Visible;
            }

            RootImageSource.ImageOpened += (s, e) =>
            {
                _IsImageLoaded = true;
                if (WindowWidth >= 720)
                    MetadataStackPanel.RequestedTheme = ElementTheme.Dark;

                if (AspectRatio == 1.5)
                    PreviewText.Visibility = Visibility.Collapsed;
            };
        }

        private void ItemControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AspectRatio = Math.Round(e.NewSize.Width / e.NewSize.Height, 1);
            if (AspectRatio < 1.5 && WindowWidth >= 720 || string.IsNullOrEmpty(ViewModel.Model.PreviewPictureUri))
                VisualStateManager.GoToState(this, nameof(TwoRows), false);
            else if (_IsImageLoaded)
                VisualStateManager.GoToState(this, nameof(Normal), false);
            
            if (WindowWidth >= 720)
            {
                if (string.IsNullOrEmpty(ViewModel.Model.PreviewPictureUri))
                    MetadataStackPanel.RequestedTheme = ElementTheme.Light;
            }
        }
    }
}
