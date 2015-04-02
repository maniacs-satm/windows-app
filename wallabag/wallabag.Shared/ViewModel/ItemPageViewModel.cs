using wallabag.Common;
using wallabag.DataModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace wallabag.ViewModel
{
    public class ItemPageViewModel : ViewModelBase
    {
        private Item _Item;
        public Item Item
        {
            get { return _Item; }
            set { Set(() => Item, ref _Item, value); }
        }

        public RelayCommand shareCommand { get; private set; }
        public RelayCommand GoBackCommand { get; private set; }

        public ItemPageViewModel()
        {
            shareCommand = new RelayCommand(() => DataTransferManager.ShowShareUI());
            GoBackCommand = new RelayCommand(() =>
            {
                Frame rootFrame = Window.Current.Content as Frame;
                if (rootFrame.CanGoBack)
                    rootFrame.GoBack();
            });
        }
    }
}