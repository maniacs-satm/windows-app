using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;
using PropertyChanged;
using Template10.Mvvm;
using wallabag.Common;
using wallabag.Models;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.Web.Http;
using static wallabag.Common.Helpers;

namespace wallabag.ViewModels
{
    [ImplementPropertyChanged]
    public class ItemViewModel : ViewModelBase
    {
        private static SQLite.SQLiteAsyncConnection conn = new SQLite.SQLiteAsyncConnection(DATABASE_PATH);

        public ItemViewModel(Item Model)
        {
            this.Model = Model;
            DeleteCommand = new DelegateCommand(async () => await DeleteAsync());
            SwitchReadStatusCommand = new DelegateCommand(async () => await SwitchReadValueAsync());
            SwitchFavoriteStatusCommand = new DelegateCommand(async () => await SwitchFavoriteValueAsync());
            ShareCommand = new DelegateCommand(() =>
            {
                DataTransferManager.GetForCurrentView().DataRequested += (s, args) =>
                {
                    var data = args.Request.Data;

                    data.SetWebLink(new Uri(Model.Url));
                    data.Properties.Title = Model.Title;
                };
                DataTransferManager.ShowShareUI();
            });
            OpenInBrowserCommand = new DelegateCommand(async () => { await Launcher.LaunchUriAsync(new Uri(Model.Url)); });

            GetIntroSentence();
            Model.Tags.CollectionChanged += Tags_CollectionChanged;
        }

        public Item Model { get; set; }
        public string ContentWithHeader { get; set; }
        public string IntroSentence { get; set; }
        public string PublishedDateFormatted { get { return Model.CreationDate.ToString("m"); } }

        public DelegateCommand DeleteCommand { get; private set; }
        public DelegateCommand SwitchReadStatusCommand { get; private set; }
        public DelegateCommand SwitchFavoriteStatusCommand { get; private set; }
        public DelegateCommand ShareCommand { get; private set; }
        public DelegateCommand OpenInBrowserCommand { get; private set; }

        public async Task CreateContentFromTemplateAsync()
        {
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Article/article.html"));
            string _template = await FileIO.ReadTextAsync(file);

            string accentColor = Application.Current.Resources["SystemAccentColor"].ToString().Remove(1, 2);
            StringBuilder styleSheetBuilder = new StringBuilder();
            styleSheetBuilder.Append("<style>");
            styleSheetBuilder.Append("hr {border-color: " + accentColor + " !important}");
            styleSheetBuilder.Append("::selection,mark {background: " + accentColor + " !important}");
            styleSheetBuilder.Append("body {");
            styleSheetBuilder.Append($"font-size:{AppSettings.FontSize}px;");
            styleSheetBuilder.Append("text-align: " + AppSettings.TextAlignment + "}");
            styleSheetBuilder.Append("</style>");

            ContentWithHeader = _template.FormatWith(new
            {
                title = Model.Title,
                content = Model.Content,
                articleUrl = Model.Url,
                hostname = Model.DomainName,
                color = AppSettings.ColorScheme,
                font = AppSettings.FontFamily,
                progress = Model.ReadingProgress,
                publishDate = string.Format("{0:d}", Model.CreationDate),
                stylesheet = styleSheetBuilder.ToString()
            });
        }
        public void GetIntroSentence()
        {
            IntroSentence = string.Empty;
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(Model.Content);

            foreach (HtmlNode node in document.DocumentNode.Descendants("p"))
            {
                if (IntroSentence.Length >= 140)
                    return;
                if (!string.IsNullOrWhiteSpace(node.InnerText))
                    IntroSentence += node.InnerText;
            }
        }
        private async void Tags_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                foreach (Tag item in e.OldItems)
                    await DeleteTagAsync(item);

            if (e.NewItems != null)
                foreach (Tag item in e.NewItems)
                    await AddTagsAsync(Model.Id, item.Label);
        }

        public async Task<bool> SwitchReadValueAsync()
        {
            if (Model.IsRead)
                Model.IsRead = false;
            else
                Model.IsRead = true;

            await conn.UpdateAsync(Model);
            return await UpdateSpecificProperty(Model.Id, "is_archived", Model.IsRead);
        }
        public async Task<bool> SwitchFavoriteValueAsync()
        {
            if (Model.IsStarred)
                Model.IsStarred = false;
            else
                Model.IsStarred = true;

            await conn.UpdateAsync(Model);
            return await UpdateSpecificProperty(Model.Id, "is_starred", Model.IsStarred);
        }
        public async Task<bool> DeleteAsync()
        {
            NavigationService?.GoBack();
            Model.IsDeleted = true;
            await conn.UpdateAsync(Model);

            var response = await ExecuteHttpRequestAsync(HttpRequestMethod.Delete, $"/entries/{Model.Id}");
            if (response.StatusCode == HttpStatusCode.Ok)
                return true;
            else
            {
                await conn.InsertAsync(new OfflineTask($"/entries/{Model.Id}", null, HttpRequestMethod.Delete));
                return false;
            }
        }

        public async static Task<ObservableCollection<Tag>> AddTagsAsync(int ItemId, string tags, bool IsOfflineAction = false)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>() {["tags"] = tags };
            var response = await ExecuteHttpRequestAsync(HttpRequestMethod.Post, $"/entries/{ItemId}/tags", parameters);

            if (response.StatusCode == HttpStatusCode.Ok)
            {
                var json = JsonConvert.DeserializeObject<Item>(response.Content.ToString());
                return json.Tags ?? new ObservableCollection<Tag>();
            }
            else
            {
                if (!IsOfflineAction)
                    await conn.InsertAsync(new OfflineTask($"/entries/{ItemId}/tags", parameters, HttpRequestMethod.Post));
                return null;
            }
        }
        public async Task<bool> DeleteTagAsync(Tag Tag)
        {
            bool result = await DeleteTagAsync(Model.Id, Tag.Id);
            if (result)
                await conn.UpdateAsync(Model);
            return result;
        }
        public static async Task<bool> DeleteTagAsync(int ItemId, int TagId, bool IsOfflineAction = false)
        {
            var response = await ExecuteHttpRequestAsync(HttpRequestMethod.Delete, $"/entries/{ItemId}/tags/{TagId}");

            if (response.StatusCode == HttpStatusCode.Ok)
                return true;
            else
            {
                if (!IsOfflineAction)
                    await conn.InsertAsync(new OfflineTask($"/entries/{ItemId}/tags/{TagId}", null, HttpRequestMethod.Delete));
                return false;
            }
        }

        public static async Task<bool> UpdateSpecificProperty(int itemId, string propertyName, object propertyValue)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add(propertyName, propertyValue);

            var response = await ExecuteHttpRequestAsync(HttpRequestMethod.Patch, $"/entries/{itemId}", parameters);
            if (response.StatusCode == HttpStatusCode.Ok)
                return true;
            else
            {
                await conn.InsertAsync(new OfflineTask($"/entries/{itemId}", parameters));
                return false;
            }
        }
    }
}
