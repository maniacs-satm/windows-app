using System.Collections.ObjectModel;
using PropertyChanged;
using wallabag.Common.Mvvm;
using wallabag.Common;
using wallabag.Models;
using wallabag.Services;

namespace wallabag.ViewModels
{
    [ImplementPropertyChanged]
    class AddItemContentDialogViewModel
    {
        public string Url { get; set; }
        public ObservableCollection<Tag> Tags { get; set; }

        public Command AddItemCommand { get; private set; }
        public Command CancelCommand { get; private set; }

        public AddItemContentDialogViewModel()
        {
            Tags = new ObservableCollection<Tag>();
            AddItemCommand = new Command(async () =>
            {
                await DataSource.AddItem(Url, Tags.ToCommaSeparatedString());
                Url = string.Empty;
                Tags.Clear();
            });
            CancelCommand = new Command(() =>
            {
                Url = string.Empty;
                Tags.Clear();
            });
        }
    }
}
