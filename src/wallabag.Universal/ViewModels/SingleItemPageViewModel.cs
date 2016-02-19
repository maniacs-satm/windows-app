using GalaSoft.MvvmLight.Messaging;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Template10.Mvvm;
using Template10.Utils;
using wallabag.Common;
using wallabag.Data.Interfaces;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using static wallabag.Common.Helpers;

namespace wallabag.ViewModels
{
    [ImplementPropertyChanged]
    public class SingleItemPageViewModel : ViewModelBase
    {
        private IDataService _dataService;
        private DataTransferManager _dataTransferManager;
        private string ContainerKey { get { return $"ReadingProgressContainer"; } }

        public ItemViewModel CurrentItem { get; set; }
        public SolidColorBrush CurrentBackground { get; set; }
        public SolidColorBrush CurrentForeground { get; set; }
        public ElementTheme AppBarRequestedTheme { get; set; }
        public double FontSize
        {
            get { return AppSettings.FontSize; }
            set { AppSettings.FontSize = value; }
        }

        public string ErrorMessage { get; set; } = string.Empty;
        public bool ErrorHappened { get; set; } = false;
        public AppBarClosedDisplayMode CommandBarClosedDisplayMode { get; set; } = AppBarClosedDisplayMode.Minimal;

        public object TextAlignButtonContent { get; set; }
        public object ColorSchemeButtonContent { get; set; }
        private PathIcon TextAlignJustifyPathIcon { get; }
            = new PathIcon() { Data = PathMarkupToGeometry("M0,1L15,1L15,2L0,2z M0,4L15,4L15,5L0,5z M0,7L15,7L15,8L0,8z M0,10L15,10L15,11L0,11z M0,13L15,13L15,14L0,14") };
        private static Geometry PathMarkupToGeometry(string markup)
        {
            string xaml = $"<Path xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'><Path.Data>{markup}</Path.Data></Path>";
            var path = Windows.UI.Xaml.Markup.XamlReader.Load(xaml) as Windows.UI.Xaml.Shapes.Path;

            // Detach the PathGeometry from the Path
            Geometry geometry = path.Data;
            path.Data = null;

            return geometry;
        }

        public DelegateCommand DownloadItemCommand { get; private set; }
        public DelegateCommand MarkItemAsReadCommand { get; private set; }
        public DelegateCommand EditTagsCommand { get; private set; }
        public DelegateCommand ShowShareUICommand { get; private set; }
        public DelegateCommand ChangeColorSchemeCommand { get; private set; }
        public DelegateCommand ChangeFontFamilyCommand { get; private set; }
        public DelegateCommand IncreaseFontSizeCommand { get; private set; }
        public DelegateCommand DecreaseFontSizeCommand { get; private set; }
        public DelegateCommand ChangeTextAlignmentCommand { get; private set; }

        public SingleItemPageViewModel(IDataService dataService)
        {
            _dataService = dataService;
            DownloadItemCommand = new DelegateCommand(async () => { await DownloadItemAsFileAsync(); });
            MarkItemAsReadCommand = new DelegateCommand(async () =>
            {
                await CurrentItem.SwitchReadValueAsync();
                if (AppSettings.NavigateBackAfterReadingAnArticle)
                    NavigationService.GoBack();
            });
            ShowShareUICommand = new DelegateCommand(() => { DataTransferManager.ShowShareUI(); });

            ChangeColorSchemeCommand = new DelegateCommand(() => ChangeColorScheme());
            EditTagsCommand = new DelegateCommand(async () =>
            {
                await Services.DialogService.ShowDialogAsync(Services.DialogService.Dialog.EditTags, CurrentItem.Model.Tags);
            });
            ChangeFontFamilyCommand = new DelegateCommand(() => ChangeFontFamily());
            IncreaseFontSizeCommand = new DelegateCommand(() =>
            {
                FontSize += 1;
                Messenger.Default.Send(new NotificationMessage("updateHTML"));
            });
            DecreaseFontSizeCommand = new DelegateCommand(() =>
            {
                FontSize -= 1;
                Messenger.Default.Send(new NotificationMessage("updateHTML"));
            });
            ChangeTextAlignmentCommand = new DelegateCommand(() =>
            {
                if (AppSettings.TextAlignment == "left")
                {
                    AppSettings.TextAlignment = "justify";
                    TextAlignButtonContent = "\uE1A2";
                }
                else
                {
                    AppSettings.TextAlignment = "left";
                    TextAlignButtonContent = TextAlignJustifyPathIcon;
                }
                Messenger.Default.Send(new NotificationMessage("updateHTML"));
            });
        }
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            CurrentItem = new ItemViewModel(await _dataService.GetItemAsync((int)parameter));

            if (AppSettings.SyncReadingProgress)
                if (ApplicationData.Current.RoamingSettings.Containers.ContainsKey(ContainerKey))
                    CurrentItem.Model.ReadingProgress = (string)ApplicationData.Current.RoamingSettings.
                        Containers[ContainerKey].
                        Values[CurrentItem.Model.Id.ToString()];

            await CurrentItem.CreateContentFromTemplateAsync();
            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().Title = CurrentItem.Model.Title;

            if (Helpers.IsPhone)
                await Windows.UI.ViewManagement.StatusBar.GetForCurrentView().HideAsync();

            if (string.IsNullOrWhiteSpace(CurrentItem.Model.Content))
            {
                ErrorHappened = true;
                ErrorMessage = LocalizedString("SingleItemArticleNotDownloaded");
                CommandBarClosedDisplayMode = AppBarClosedDisplayMode.Hidden;
            }
            else if (CurrentItem.Model.Content == "<p>Unable to retrieve readable content.</p>")
            {
                ErrorHappened = true;
                ErrorMessage = LocalizedString("SingleItemNoReadableContentFound");
                CommandBarClosedDisplayMode = AppBarClosedDisplayMode.Hidden;
            }

            _dataTransferManager = DataTransferManager.GetForCurrentView();
            _dataTransferManager.DataRequested += DataRequested;

            if (AppSettings.TextAlignment == "left")
                TextAlignButtonContent = "\uE1A2";
            else
                TextAlignButtonContent = TextAlignJustifyPathIcon;

            if (AppSettings.ColorScheme == "light")
            {
                ColorSchemeButtonContent = "\uE708";
                CurrentBackground = ColorHelper.FromArgb(255, 255, 255, 255).ToSolidColorBrush();
                CurrentForeground = ColorHelper.FromArgb(255, 68, 68, 68).ToSolidColorBrush();
                AppBarRequestedTheme = ElementTheme.Light;
            }
            else {
                ColorSchemeButtonContent = "\uE706";
                CurrentBackground = new SolidColorBrush(ColorHelper.FromArgb(255, 51, 51, 51));
                CurrentForeground = new SolidColorBrush(ColorHelper.FromArgb(255, 204, 204, 204));
                AppBarRequestedTheme = ElementTheme.Dark;
            }
        }
        public override async Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().Title = string.Empty;
            await _dataService.UpdateItemAsync(CurrentItem.Model);

            if (AppSettings.SyncReadingProgress)
                ApplicationData.Current.RoamingSettings.CreateContainer(ContainerKey,
                    ApplicationDataCreateDisposition.Always).Values[CurrentItem.Model.Id.ToString()] = CurrentItem.Model.ReadingProgress;

            if (IsPhone)
                await Windows.UI.ViewManagement.StatusBar.GetForCurrentView().ShowAsync();

            _dataTransferManager.DataRequested -= DataRequested;

            GC.Collect();
        }

        private void DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var deferral = args.Request.GetDeferral();

            if (CurrentItem != null)
            {
                var data = args.Request.Data;

                data.SetWebLink(new Uri(CurrentItem.Model.Url));
                data.Properties.Title = CurrentItem.Model.Title;
            }

            deferral.Complete();
        }

        public void ChangeColorScheme()
        {
            if (AppSettings.ColorScheme == "light")
            {
                AppSettings.ColorScheme = "dark";
                ColorSchemeButtonContent = "\uE706";
                CurrentBackground = new SolidColorBrush(ColorHelper.FromArgb(255, 51, 51, 51));
                CurrentForeground = new SolidColorBrush(ColorHelper.FromArgb(255, 204, 204, 204));
                AppBarRequestedTheme = ElementTheme.Dark;
            }
            else
            {
                AppSettings.ColorScheme = "light";
                ColorSchemeButtonContent = "\uE708";
                CurrentBackground = ColorHelper.FromArgb(255, 255, 255, 255).ToSolidColorBrush();
                CurrentForeground = ColorHelper.FromArgb(255, 68, 68, 68).ToSolidColorBrush();
                AppBarRequestedTheme = ElementTheme.Light;
            }

            Messenger.Default.Send(new NotificationMessage("updateHTML"));
        }
        public void ChangeFontFamily()
        {
            if (AppSettings.FontFamily == "serif")
                AppSettings.FontFamily = "sans";
            else
                AppSettings.FontFamily = "serif";

            Messenger.Default.Send(new NotificationMessage("updateHTML"));
        }

        // TODO: Add translations.
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

                        AddHttpHeadersAsync(http);

                        var response = await http.GetAsync(downloadUrl);
                        if (response.IsSuccessStatusCode)
                            await FileIO.WriteBufferAsync(file, await response.Content.ReadAsBufferAsync());
                    }
                }
                catch { }
        }
    }
}
