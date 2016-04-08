using HtmlAgilityPack;
using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;
using wallabag.Common;
using wallabag.Data.Interfaces;
using wallabag.Data.Models;
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
    public class ItemViewModel : ViewModelBase, IComparable
    {
        private IDataService _dataService;
        public ItemViewModel(Item Model, IDataService dataService)
        {
            this.Model = Model;
            this._dataService = dataService;
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
            TagsAreExisting = (Model.Tags.Count > 0) ? true : false;
        }

        public Item Model { get; set; }
        public string ContentWithHeader { get; set; }
        public string IntroSentence { get; set; }
        public string PublishedDateFormatted { get { return Model.CreationDate.ToString("m"); } }
        public string TagsString { get { return Model.Tags.ToCommaSeparatedString().Replace(",", ", "); } }
        public bool TagsAreExisting { get; set; }

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

            ContentWithHeader = _template.FormatWith(new
            {
                title = Model.Title,
                content = document.DocumentNode.OuterHtml,
                articleUrl = Model.Url,
                hostname = Model.DomainName,
                color = AppSettings.ColorScheme,
                font = AppSettings.FontFamily,
                progress = Model.ReadingProgress ?? "0",
                publishDate = string.Format("{0:d}", Model.CreationDate),
                stylesheet = styleSheetBuilder.ToString(),
                tags = GenerateHtmlFormattedTagsList()
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
        public string GenerateHtmlFormattedTagsList()
        {
            string result = string.Empty;

            if (Model.Tags.Count > 0)
            {
                result = "<ul class=\"tags\" id=\"wallabag-tag-list\">";
                foreach (var item in Model.Tags)
                    result += $"<li>{item.Label}</li>";
                result += "</ul>";
            }

            return result;
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

            TagsAreExisting = (Model.Tags.Count > 0) ? true : false;
        }

        public async Task SwitchReadValueAsync()
        {
            if (Model.IsRead)
            {
                Model.IsRead = false;
                await OfflineTask.AddToQueueAsync(Model, OfflineTask.OfflineTaskAction.UnmarkAsRead);
            }
            else
            {
                Model.IsRead = true;
                await OfflineTask.AddToQueueAsync(Model, OfflineTask.OfflineTaskAction.MarkAsRead);

                ApplicationData.Current.RoamingSettings.Containers["ReadingProgressContainer"].Values.Remove(Model.Id.ToString());
            }
            await Model.UpdateAsync();
        }
        public async Task SwitchFavoriteValueAsync()
        {
            if (Model.IsStarred)
            {
                Model.IsStarred = false;
                await OfflineTask.AddToQueueAsync(Model, OfflineTask.OfflineTaskAction.UnmarkAsFavorite);
            }
            else
            {
                Model.IsStarred = true;
                await OfflineTask.AddToQueueAsync(Model, OfflineTask.OfflineTaskAction.MarkAsFavorite);
            }
            await Model.UpdateAsync();
        }
        public async Task DeleteAsync()
        {
            NavigationService?.GoBack();
            await OfflineTask.AddToQueueAsync(Model, OfflineTask.OfflineTaskAction.Delete);
            await _dataService.DeleteItemAsync(Model);
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
                await OfflineTask.AddToQueueAsync(Model, OfflineTask.OfflineTaskAction.AddTags, Tags.ToCommaSeparatedString());
                return false;
            }
        }
        public async Task<bool> DeleteTagAsync(Tag Tag)
        {
            if (Tag.Id == -1)
            {
                OfflineTask task = (await _dataService.GetOfflineTasksAsync()).Where(t => t.RequestParameters.ContainsValue(Tag.Label)).FirstOrDefault();
                if (task != null)
                    await task.DeleteFromQueueAsync();

                return true;
            }

            var response = await ExecuteHttpRequestAsync(HttpRequestMethod.Delete, $"/entries/{Model.Id}/tags/{Tag.Id}");

            if (response.StatusCode == HttpStatusCode.Ok)
                return true;
            else
            {
                await OfflineTask.AddToQueueAsync(Model, OfflineTask.OfflineTaskAction.DeleteTag, Tag);
                return false;
            }
        }

        public override bool Equals(object obj)
        {
            if (this.Model.Id == (obj as ItemViewModel).Model.Id)
                return true;
            else return false;
        }

        public override int GetHashCode() => Model.Id;

        public int CompareTo(object obj)
        {
            if (this.Model.Id == (obj as ItemViewModel).Model.Id) return 0;
            else return this.Model.Id.CompareTo((obj as ItemViewModel).Model.Id);
        }
    }
}
