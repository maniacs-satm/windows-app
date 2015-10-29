using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace wallabag.Controls
{
    public sealed partial class ShareTextControl : UserControl
    {
        public ShareTextControl()
        {
            this.InitializeComponent();
        }

        #region Properties 
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(ShareTextControl), new PropertyMetadata(DependencyProperty.UnsetValue));



        public string DomainName
        {
            get { return (string)GetValue(DomainNameProperty); }
            set { SetValue(DomainNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DomainName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DomainNameProperty =
            DependencyProperty.Register("DomainName", typeof(string), typeof(ShareTextControl), new PropertyMetadata(DependencyProperty.UnsetValue));


        public string SelectionContent
        {
            get { return (string)GetValue(SelectionContentProperty); }
            set { SetValue(SelectionContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectionContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectionContentProperty =
            DependencyProperty.Register("SelectionContent", typeof(string), typeof(ShareTextControl), new PropertyMetadata(DependencyProperty.UnsetValue));

        #endregion

        public async Task<RenderTargetBitmap> CaptureToStreamAsync(IRandomAccessStream stream)
        {
            var renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(RootGrid, (int)RootGrid.ActualWidth * 2, (int)RootGrid.ActualHeight * 2);

            var pixels = await renderTargetBitmap.GetPixelsAsync();

            var logicalDpi = DisplayInformation.GetForCurrentView().LogicalDpi;
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            encoder.SetPixelData(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Ignore,
                (uint)renderTargetBitmap.PixelWidth,
                (uint)renderTargetBitmap.PixelHeight,
                logicalDpi,
                logicalDpi,
                pixels.ToArray());

            await encoder.FlushAsync();

            return renderTargetBitmap;
        }
    }
}
