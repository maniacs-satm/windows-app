using System.Collections.ObjectModel;
using PropertyChanged;
using wallabag.Common.MVVM;
using wallabag.DataModel;
using wallabag.Common;

namespace wallabag.ViewModels
{
    [ImplementPropertyChanged]
    class AddItemContentDialogViewModel
    {
        public string Url { get; set; }
        public ObservableCollection<Tag> Tags { get; set; }

        public RelayCommand AddItemCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }

        public AddItemContentDialogViewModel()
        {
            Tags = new ObservableCollection<Tag>();
            AddItemCommand = new RelayCommand(async () =>
            {
                await DataSource.AddItem(Url, Tags.ToCommaSeparatedString());
                Url = string.Empty;
                Tags.Clear();
            });
            CancelCommand = new RelayCommand(() =>
            {
                Url = string.Empty;
                Tags.Clear();
            });
        }
    }
}
