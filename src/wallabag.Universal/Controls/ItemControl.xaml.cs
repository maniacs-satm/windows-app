using System;
using wallabag.Common;
using wallabag.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace wallabag.Controls
{
    public sealed partial class ItemControl : UserControl
    {
        public ItemViewModel ViewModel { get { return DataContext as ItemViewModel; } }

        private double WindowWidth { get { return Window.Current.CoreWindow.Bounds.Width; } }
        private double AspectRatio { get; set; } = 1.5;
        private bool _IsImageLoaded = false;

        public ItemControl()
        {
            InitializeComponent();
            SizeChanged += ItemControl_SizeChanged;

            Loading += (s, e) =>
            {
                if (AppSettings.UseComplexItemStyle)
                {
                    ComplexTrigger.MinWindowWidth = 720;
                    Wide.StateTriggers.Clear();
                    VisualStateManager.GoToState(this, nameof(NormalComplex), false);
                }
                else
                {
                    NormalTrigger.MinWindowWidth = 720;
                    WideComplex.StateTriggers.Clear();
                    VisualStateManager.GoToState(this, nameof(Normal), false);
                }
            };

            RootImageSource.ImageOpened += (s, e) =>
            {
                _IsImageLoaded = true;
                UpdateLayoutSpecificProperties();

                if (AspectRatio == 1.5)
                    PreviewText.Visibility = Visibility.Collapsed;
            };

            if (!AppSettings.UseComplexItemStyle)
            {
                PointerEntered += (s, e) =>
                {
                    if (WindowWidth >= 720)
                        ShowPreviewTextStoryboard.Begin();
                };
                PointerExited += (s, e) =>
                 {
                     if (WindowWidth >= 720)
                         HidePreviewTextStoryboard.Begin();
                 };
            }
        }

        private void ItemControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AspectRatio = Math.Round(e.NewSize.Width / e.NewSize.Height, 1);
            if (AspectRatio < 1.5 && WindowWidth >= 720)
                VisualStateManager.GoToState(this, nameof(TwoRows), false);
            else if (_IsImageLoaded && !AppSettings.UseComplexItemStyle)
                VisualStateManager.GoToState(this, nameof(Normal), false);
            else if (_IsImageLoaded && AppSettings.UseComplexItemStyle)
                VisualStateManager.GoToState(this, nameof(NormalComplex), false);

            if (WindowWidth >= 720)
            {
                if (string.IsNullOrEmpty(ViewModel.Model.PreviewPictureUri) && AppSettings.UseComplexItemStyle)
                    MetadataStackPanel.RequestedTheme = ElementTheme.Light;
            }

            UpdateLayoutSpecificProperties();
        }

        private void UpdateLayoutSpecificProperties()
        {
            if (WindowWidth >= 720)
            {
                MetadataStackPanel.RequestedTheme = ElementTheme.Dark;

                if (AppSettings.UseComplexItemStyle && _IsImageLoaded)
                {
                    var height = this.ActualHeight / 2;

                    image.MaxHeight = height;
                    OverlayGrid.Height = height;
                }
            }
            else
                MetadataStackPanel.RequestedTheme = ElementTheme.Default;

            if (string.IsNullOrEmpty(ViewModel.Model.PreviewPictureUri) && AppSettings.UseComplexItemStyle)
            {
                MetadataStackPanel.RequestedTheme = ElementTheme.Default;
                PreviewText.Visibility = Visibility.Visible;
            }
        }
    }
}
