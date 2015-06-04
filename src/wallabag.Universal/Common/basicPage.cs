using wallabag.Common.Navigation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace wallabag.Common
{
    /// <summary>
    /// To avoid duplicate code across all the pages, this class is a base class for all pages.
    /// </summary>
    public class basicPage : Page
    {
        public NavigationHelper navigationHelper;
        private const string ViewModelPageKey = "ViewModel";

        public basicPage()
        {
            Loaded += basicPage_Loaded;
            Unloaded += basicPage_Unloaded;
            navigationHelper = new NavigationHelper(this);
            navigationHelper.LoadState += navigationHelper_LoadState;
            navigationHelper.SaveState += navigationHelper_SaveState;
        }

        void basicPage_Loaded(object sender, RoutedEventArgs e) { Window.Current.SizeChanged += Window_SizeChanged; }
        void basicPage_Unloaded(object sender, RoutedEventArgs e) { Window.Current.SizeChanged -= Window_SizeChanged; }

        void Window_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            ChangedSize(e.Size.Width, e.Size.Height);
        }

        /// <summary>
        /// Could be easily overrided, but is not required.
        /// </summary>
        protected virtual void ChangedSize(double width, double height) { }

        void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            SaveState(e);
        }
        void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            LoadState(e);
        }

        protected virtual void LoadState(LoadStateEventArgs e) { }
        protected virtual void SaveState(SaveStateEventArgs e) { }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
            ChangedSize(Window.Current.Bounds.Width, Window.Current.Bounds.Height);
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }
    }
}
