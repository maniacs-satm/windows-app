using System;
using wallabag.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace wallabag.Controls
{
    public sealed partial class ItemControl : UserControl
    {
        private double WindowWidth { get { return Window.Current.CoreWindow.Bounds.Width; } }
        public ItemViewModel ViewModel { get { return DataContext as ItemViewModel; } }

        public ItemControl()
        {
            InitializeComponent();
            SizeChanged += ItemControl_SizeChanged;
        }

        private void ItemControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var aspectRatio = Math.Round(e.NewSize.Width / e.NewSize.Height, 1);
            if (aspectRatio < 1.5 && WindowWidth >= 720 || string.IsNullOrEmpty(ViewModel.Model.PreviewPictureUri))
                VisualStateManager.GoToState(this, nameof(TwoRows), false);
            else
                VisualStateManager.GoToState(this, nameof(Normal), false);
        }
    }
}
