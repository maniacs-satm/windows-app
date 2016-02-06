using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
using wallabag.Data.Models;

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
            (Model.Tags as ObservableCollection<Tag>).CollectionChanged += Tags_CollectionChanged;
        }

        public Item Model { get; set; }
        public string ContentWithHeader { get; set; }
        public string IntroSentence { get; set; }
        public string PublishedDateFormatted { get { return Model.CreationDate.ToString("m"); } }
        public string TagsString { get { return Model.Tags.ToCommaSeparatedString().Replace(",", ", "); } }

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

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(Model.Content);

            foreach (HtmlNode node in document.DocumentNode.Descendants("img"))
            {
                var imageSource = node.Attributes["src"].Value;
                node.Attributes.Add("data-src", imageSource);
                node.Attributes["src"].Value = string.Empty;
            }

            ContentWithHeader = _template.FormatWith(new
            {
                title = Model.Title,
                content = document.DocumentNode.OuterHtml,
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
            {
                IList<Tag> newItems = new List<Tag>();
                foreach (Tag item in e.NewItems)
                    newItems.Add(item);

                await AddTagsAsync(newItems);
            }
        }

        public async Task<bool> SwitchReadValueAsync()
        {
            if (Model.IsRead)
                Model.IsRead = false;
            else
                Model.IsRead = true;

            await Model.UpdateAsync();
            return await UpdateSpecificProperty(Model.Id, "is_archived", Model.IsRead);
        }
        public async Task<bool> SwitchFavoriteValueAsync()
        {
            if (Model.IsStarred)
                Model.IsStarred = false;
            else
                Model.IsStarred = true;

            await Model.UpdateAsync();
            return await UpdateSpecificProperty(Model.Id, "is_starred", Model.IsStarred);
        }
        public async Task<bool> DeleteAsync()
        {
            NavigationService?.GoBack();
            Model.IsDeleted = true;
            await Model.UpdateAsync();

            var response = await ExecuteHttpRequestAsync(HttpRequestMethod.Delete, $"/entries/{Model.Id}");
            if (response.StatusCode == HttpStatusCode.Ok)
                return true;
            else
            {
                await OfflineTask.AddTaskAsync(Model, OfflineTask.OfflineTaskAction.Delete, $"/entries/{Model.Id}", null, HttpRequestMethod.Delete);
                return false;
            }
        }

        public async Task<bool> AddTagsAsync(IList<Tag> Tags)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>() { ["tags"] = Tags.ToCommaSeparatedString() };
            var response = await ExecuteHttpRequestAsync(HttpRequestMethod.Post, $"/entries/{Model.Id}/tags", parameters);

            if (response.StatusCode == HttpStatusCode.Ok)
            {
                var json = JsonConvert.DeserializeObject<Item>(response.Content.ToString());

                foreach (var tag in json.Tags)
                {
                    var existingTag = Model.Tags.Where(t => t.Label == tag.Label).FirstOrDefault();
                    if (existingTag != null)
                        existingTag.Id = tag.Id;
                }

                await Model.UpdateAsync();
                return true;
            }
            else
            {
                await OfflineTask.AddTaskAsync(Model, OfflineTask.OfflineTaskAction.ModifyTags, $"/entries/{Model.Id}/tags", parameters, HttpRequestMethod.Post);
                return false;
            }
        }
        public async Task<bool> DeleteTagAsync(Tag Tag)
        {
            if (Tag.Id == -1)
            {
                OfflineTask task = (await conn.Table<OfflineTask>().ToListAsync()).Where(t => t.RequestParameters.ContainsValue(Tag.Label)).FirstOrDefault();
                if (task != null)
                    await conn.DeleteAsync(task);

                return true;
            }

            var response = await ExecuteHttpRequestAsync(HttpRequestMethod.Delete, $"/entries/{Model.Id}/tags/{Tag.Id}");

            if (response.StatusCode == HttpStatusCode.Ok)
                return true;
            else
            {
                await OfflineTask.AddTaskAsync(Model, OfflineTask.OfflineTaskAction.ModifyTags, $"/entries/{Model.Id}/tags/{Tag.Id}", null, HttpRequestMethod.Delete);
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
                // TODO: Set Action based on the propertyName
                await OfflineTask.AddTaskAsync(new Item() { Id = itemId }, OfflineTask.OfflineTaskAction.ModifyTags, $"/entries/{itemId}", parameters);
                return false;
            }
        }

        public override bool Equals(object obj)
        {
            if (this.Model.Id == (obj as ItemViewModel).Model.Id)
                return true;
            else return false;
        }
    }
}
