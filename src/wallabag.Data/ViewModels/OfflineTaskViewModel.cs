using PropertyChanged;
using Template10.Mvvm;
using wallabag.Data.Models;

namespace wallabag.Data.ViewModels
{
    [ImplementPropertyChanged]
    public class OfflineTaskViewModel : ViewModelBase
    {
        public OfflineTask Model { get; set; }

        public string ItemTitle { get; set; }
        public string ActionLogo { get; set; }

        public OfflineTaskViewModel(OfflineTask Model)
        {
            this.Model = Model;
        }
    }
}
