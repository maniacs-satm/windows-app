using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PropertyChanged;
using wallabag.Common;
using wallabag.Common.Mvvm;
using wallabag.DataModel;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Web.Http;

namespace wallabag.ViewModels
{
    [ImplementPropertyChanged]
    public class SingleItemPageViewModel : ViewModelBase
     {
        public override string ViewModelIdentifier { get; set; } = "SingleItemPageViewModel";

        public ItemViewModel CurrentItem { get; set; }

        public Command DownloadItemCommand { get; private set; }
        public SingleItemPageViewModel()
        {
            DownloadItemCommand = new Command(async () => { await DownloadItem(); });
        }

        public async Task DownloadItem()
        {
            // Let the user select the download path
            var picker = new FileSavePicker()
            {
                SuggestedFileName = CurrentItem.Model.Title,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                DefaultFileExtension = ".pdf"
            };
            picker.FileTypeChoices.Add("PDF document", new List<string>() { ".pdf" });
            picker.FileTypeChoices.Add("Epub file", new List<string>() { ".epub" });
            picker.FileTypeChoices.Add("Mobi file", new List<string>() { ".mobi" });
            StorageFile file = await picker.PickSaveFileAsync();

            // Download the file
            if (file != null)
                using (HttpClient http = new HttpClient())
                {
                    // TODO: Currently just downloading the login page :/
                    Uri downloadUrl =new Uri( $"{AppSettings.Instance.wallabagUrl}/view/{CurrentItem.Model.Id}?{file.FileType}&method=id&value={CurrentItem.Model.Id}");

                    await Helpers.AddHeaders(http);

                    var response = await http.GetAsync(downloadUrl);
                    if (response.IsSuccessStatusCode)
                        await FileIO.WriteBufferAsync(file, await response.Content.ReadAsBufferAsync());
                }
        }
    }
}
