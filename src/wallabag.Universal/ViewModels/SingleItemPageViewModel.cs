using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using PropertyChanged;
using Template10.Mvvm;
using wallabag.Common;
using wallabag.Models;
using wallabag.Services;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

namespace wallabag.ViewModels
{
    [ImplementPropertyChanged]
    public class SingleItemPageViewModel : ViewModelBase
    {
        public ItemViewModel CurrentItem { get; set; }
        private string ContainerKey { get { return $"ReadingProgressContainer-{new Uri(AppSettings.wallabagUrl).Host}"; } }

        public SolidColorBrush AppBarBackground { get; set; }
        public SolidColorBrush AppBarForeground { get; set; }
        public ElementTheme AppBarRequestedTheme { get; set; }

        public ICollection<Tag> ItemTags { get; set; }

        public double FontSize
        {
            get { return AppSettings.FontSize; }
            set { AppSettings.FontSize = value; }
        }

        public DelegateCommand DownloadItemCommand { get; private set; }
        public DelegateCommand MarkItemAsReadCommand { get; private set; }

        public SingleItemPageViewModel()
        {
            DownloadItemCommand = new DelegateCommand(async () => { await DownloadItemAsFileAsync(); });
            MarkItemAsReadCommand = new DelegateCommand(async () =>
            {
                await CurrentItem.SwitchReadValueAsync();
                if (AppSettings.NavigateBackAfterReadingAnArticle)
                    NavigationService.GoBack();
            });
        }

        private async void SingleItemPageViewModel_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (Tag t in e.NewItems)
                    await ItemViewModel.AddTagsAsync(CurrentItem.Model.Id, t.Label);

            if (e.OldItems != null)
                foreach (Tag t in e.OldItems)
                    await ItemViewModel.DeleteTagAsync(CurrentItem.Model.Id, t.Id);
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().Title = string.Empty;
            await new SQLite.SQLiteAsyncConnection(Helpers.DATABASE_PATH).UpdateAsync(CurrentItem.Model);

            if (AppSettings.SyncReadingProgress)
                ApplicationData.Current.RoamingSettings.CreateContainer(ContainerKey,
                    ApplicationDataCreateDisposition.Always).Values[CurrentItem.Model.Id.ToString()] = CurrentItem.Model.ReadingProgress;

            if (Helpers.IsPhone)
                await Windows.UI.ViewManagement.StatusBar.GetForCurrentView().ShowAsync();
        }
        public override async void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (!string.IsNullOrWhiteSpace(parameter as string))
            {
                CurrentItem = new ItemViewModel(await DataService.GetItemAsync(int.Parse(parameter as string)));

                if (AppSettings.SyncReadingProgress)
                    if (ApplicationData.Current.RoamingSettings.Containers.ContainsKey(ContainerKey))
                        CurrentItem.Model.ReadingProgress = (string)ApplicationData.Current.RoamingSettings.
                            Containers[ContainerKey].
                            Values[CurrentItem.Model.Id.ToString()];

                await CurrentItem.CreateContentFromTemplateAsync();
                Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().Title = CurrentItem.Model.Title;

                if (Helpers.IsPhone)
                    await Windows.UI.ViewManagement.StatusBar.GetForCurrentView().HideAsync();

                ItemTags = CurrentItem.Model.Tags;
                (ItemTags as ObservableCollection<Tag>).CollectionChanged += SingleItemPageViewModel_CollectionChanged;
            }
            ChangeAppBarBrushes();
        }

        public void ChangeAppBarBrushes()
        {
            switch (AppSettings.ColorScheme)
            {
                case "light":
                    AppBarBackground = new SolidColorBrush(ColorHelper.FromArgb(255, 255, 255, 255));
                    AppBarForeground = new SolidColorBrush(ColorHelper.FromArgb(255, 68, 68, 68));
                    AppBarRequestedTheme = ElementTheme.Light;
                    break;
                case "sepia":
                    AppBarBackground = new SolidColorBrush(ColorHelper.FromArgb(255, 245, 245, 220));
                    AppBarForeground = new SolidColorBrush(ColorHelper.FromArgb(255, 128, 0, 0));
                    AppBarRequestedTheme = ElementTheme.Light;
                    break;
                case "dark":
                    AppBarBackground = new SolidColorBrush(ColorHelper.FromArgb(255, 51, 51, 51));
                    AppBarForeground = new SolidColorBrush(ColorHelper.FromArgb(255, 204, 204, 204));
                    AppBarRequestedTheme = ElementTheme.Dark;
                    break;
                case "black":
                    AppBarBackground = new SolidColorBrush(ColorHelper.FromArgb(255, 0, 0, 0));
                    AppBarForeground = new SolidColorBrush(ColorHelper.FromArgb(255, 178, 178, 178));
                    AppBarRequestedTheme = ElementTheme.Dark;
                    break;
            }
        }
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
                try
                {
                    using (HttpClient http = new HttpClient())
                    {
                        // TODO: Currently just downloading the login page :/
                        Uri downloadUrl = new Uri($"{AppSettings.wallabagUrl}/view/{CurrentItem.Model.Id}?{file.FileType}&method=id&value={CurrentItem.Model.Id}");

                        Helpers.AddHttpHeadersAsync(http);

                        var response = await http.GetAsync(downloadUrl);
                        if (response.IsSuccessStatusCode)
                            await FileIO.WriteBufferAsync(file, await response.Content.ReadAsBufferAsync());
                    }
                }
                catch { }
        }
    }
}
