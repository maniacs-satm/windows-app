using System;
using System.Collections.Generic;
using System.Text;
using PropertyChanged;
using wallabag.DataModel;
using wallabag.Common;
using Windows.System;

namespace wallabag.ViewModels
{
    [ImplementPropertyChanged]
    class SingleItemPageViewModel : ViewModelBase
    {
        public ItemViewModel CurrentItem { get; set; }

        public RelayCommand DownloadItemAsPDFCommand { get; private set; }
        public RelayCommand DownloadItemAsMobiCommand { get; private set; }
        public RelayCommand DownloadItemAsEpubCommand { get; private set; }
        public SingleItemPageViewModel()
        {
            DownloadItemAsPDFCommand = new RelayCommand(async () => { await Launcher.LaunchUriAsync(new Uri($"{AppSettings.WallabagUrl}/view/{CurrentItem.Model.Id}?pdf&method=id&value={CurrentItem.Model.Id}")); });
            DownloadItemAsMobiCommand = new RelayCommand(async () => { await Launcher.LaunchUriAsync(new Uri($"{AppSettings.WallabagUrl}/view/{CurrentItem.Model.Id}?mobi&method=id&value={CurrentItem.Model.Id}")); });
            DownloadItemAsEpubCommand = new RelayCommand(async () => { await Launcher.LaunchUriAsync(new Uri($"{AppSettings.WallabagUrl}/view/{CurrentItem.Model.Id}?epub&method=id&value={CurrentItem.Model.Id}")); });
        }
    }
}
