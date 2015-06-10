using PropertyChanged;
using wallabag.Common.MVVM;

namespace wallabag.ViewModels
{
    [ImplementPropertyChanged]
    class AddItemContentDialogViewModel
    {
        public string Url { get; set; }
        public string Tags { get; set; }

        public RelayCommand AddItemCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }

        public AddItemContentDialogViewModel()
        {
            AddItemCommand = new RelayCommand(async () => await DataModel.DataSource.AddItem(Url, Tags));
            CancelCommand = new RelayCommand(() =>
            {
                Url = string.Empty;
                Tags = string.Empty;
            });
        }
    }
}
