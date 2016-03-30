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
        public static string ContainerKey { get { return "ReadingProgressContainer"; } }

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

        public DelegateCommand MarkItemAsReadCommand { get; private set; }
        public DelegateCommand EditTagsCommand { get; private set; }
        public DelegateCommand ShowShareUICommand { get; private set; }
        public DelegateCommand<string> ChangeColorSchemeCommand { get; private set; }
        public DelegateCommand ChangeFontFamilyCommand { get; private set; }
        public DelegateCommand IncreaseFontSizeCommand { get; private set; }
        public DelegateCommand DecreaseFontSizeCommand { get; private set; }
        public DelegateCommand ChangeTextAlignmentCommand { get; private set; }
        public DelegateCommand DeleteItemCommand { get; private set; }

        public SingleItemPageViewModel(IDataService dataService)
        {
            _dataService = dataService;
            MarkItemAsReadCommand = new DelegateCommand(async () =>
            {
                await CurrentItem.SwitchReadValueAsync();
                if (AppSettings.NavigateBackAfterReadingAnArticle)
                    NavigationService.GoBack();
            });
            ShowShareUICommand = new DelegateCommand(() => { DataTransferManager.ShowShareUI(); });

            ChangeColorSchemeCommand = new DelegateCommand<string>(color => ChangeColorScheme(color));
            EditTagsCommand = new DelegateCommand(async () =>
            {
                var result = await Services.DialogService.ShowDialogAsync(Services.DialogService.Dialog.EditTags, CurrentItem.Model.Tags);
                if (result == ContentDialogResult.Primary)
                    Messenger.Default.Send(new NotificationMessage("updateTagsHtml"));
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
                    AppSettings.TextAlignment = "justify";
                else
                    AppSettings.TextAlignment = "left";

                Messenger.Default.Send(new NotificationMessage("updateHTML"));
            });
            DeleteItemCommand = new DelegateCommand(async () =>
            {
                if (NavigationService.CanGoBack)
                    NavigationService.GoBack();
                await CurrentItem.DeleteAsync();
            });
        }
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            CurrentItem = new ItemViewModel(await _dataService.GetItemAsync((int)parameter), _dataService);

            Messenger.Default.Register<NotificationMessage<string>>(this, message =>
            {
                if (message.Notification == "readingProgress")
                    CurrentItem.Model.ReadingProgress = message.Content;
            });

            if (AppSettings.SyncReadingProgress)
                if (ApplicationData.Current.RoamingSettings.Containers.ContainsKey(ContainerKey))
                    CurrentItem.Model.ReadingProgress = (string)ApplicationData.Current.RoamingSettings.
                        Containers[ContainerKey].
                        Values[CurrentItem.Model.Id.ToString()];

            await CurrentItem.CreateContentFromTemplateAsync();
            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().Title = CurrentItem.Model.Title;

            if (IsPhone)
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
            else
            {
                ErrorHappened = false;
                CommandBarClosedDisplayMode = AppBarClosedDisplayMode.Minimal;
            }

            _dataTransferManager = DataTransferManager.GetForCurrentView();
            _dataTransferManager.DataRequested += DataRequested;

            ChangeColorScheme(string.Empty);
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

            Messenger.Default.Unregister(this);
            _dataTransferManager.DataRequested -= DataRequested;
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

        public void ChangeColorScheme(string color)
        {
            var colorScheme = color;

            if (string.IsNullOrEmpty(color))
                colorScheme = AppSettings.ColorScheme;

            if (colorScheme == "dark")
            {
                CurrentBackground = ColorHelper.FromArgb(255, 51, 51, 51).ToSolidColorBrush();
                CurrentForeground = ColorHelper.FromArgb(255, 204, 204, 204).ToSolidColorBrush();
                AppBarRequestedTheme = ElementTheme.Dark;
            }
            else if (colorScheme == "light")
            {
                CurrentBackground = ColorHelper.FromArgb(255, 255, 255, 255).ToSolidColorBrush();
                CurrentForeground = ColorHelper.FromArgb(255, 68, 68, 68).ToSolidColorBrush();
                AppBarRequestedTheme = ElementTheme.Light;
            }
            else if (colorScheme == "sepia")
            {
                CurrentBackground = Colors.Beige.ToSolidColorBrush();
                CurrentForeground = Colors.Maroon.ToSolidColorBrush();
                AppBarRequestedTheme = ElementTheme.Light;
            }

            if (!string.IsNullOrEmpty(color))
            {
                AppSettings.ColorScheme = color;
                Messenger.Default.Send(new NotificationMessage("updateHTML"));
            }
        }
        public void ChangeFontFamily()
        {
            if (AppSettings.FontFamily == "serif")
                AppSettings.FontFamily = "sans";
            else
                AppSettings.FontFamily = "serif";

            Messenger.Default.Send(new NotificationMessage("updateHTML"));
        }
    }
}
