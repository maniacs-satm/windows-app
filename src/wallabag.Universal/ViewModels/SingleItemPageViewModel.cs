using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PropertyChanged;
using wallabag.Common;
using wallabag.Common.Mvvm;
using wallabag.Services;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

namespace wallabag.ViewModels
{
    [ImplementPropertyChanged]
    public class SingleItemPageViewModel : ViewModelBase
    {
        public override string ViewModelIdentifier { get; set; } = "SingleItemPageViewModel";

        public ItemViewModel CurrentItem { get; set; }

        public double FontSize
        {
            get { return AppSettings.FontSize; }
            set { AppSettings.FontSize = value; }
        }
        public double LineHeight
        {
            get { return AppSettings.LineHeight; }
            set { AppSettings.LineHeight = value; }
        }

        public Command DownloadItemCommand { get; private set; }
        public Command MarkItemAsReadCommand { get; private set; }
        public async Task DownloadItemAsFileAsync()
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
                    Uri downloadUrl =new Uri( $"{AppSettings.wallabagUrl}/view/{CurrentItem.Model.Id}?{file.FileType}&method=id&value={CurrentItem.Model.Id}");

                    await Helpers.AddHttpHeadersAsync(http);

                    var response = await http.GetAsync(downloadUrl);
                    if (response.IsSuccessStatusCode)
                        await FileIO.WriteBufferAsync(file, await response.Content.ReadAsBufferAsync());
                }
        }

        public SingleItemPageViewModel()
        {
            DownloadItemCommand = new Command(async () => { await DownloadItemAsFileAsync(); });
            MarkItemAsReadCommand = new Command(async () =>
            {
                await CurrentItem.SwitchReadValueAsync();
                if (AppSettings.NavigateBackAfterReadingAnArticle)
                    Services.NavigationService.NavigationService.ApplicationNavigationService.GoBack();
            });
        }

        public override async void OnNavigatedTo(string parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (!string.IsNullOrWhiteSpace(parameter))
            {
                CurrentItem = new ItemViewModel(await DataService.GetItemAsync(int.Parse(parameter)));
                await CurrentItem.CreateContentFromTemplateAsync();

                Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().Title = CurrentItem.Model.Title;
            }
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().Title = string.Empty;

            return base.OnNavigatedFromAsync(state, suspending);
        }

    }
}
