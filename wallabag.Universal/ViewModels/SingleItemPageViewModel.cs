using PropertyChanged;
using wallabag.DataModel;

namespace wallabag.ViewModels
{
    [ImplementPropertyChanged]
    class SingleItemPageViewModel
    {
        public ItemViewModel CurrentItem { get; set; }
        public SingleItemPageViewModel()
        {

        }
    }
}
