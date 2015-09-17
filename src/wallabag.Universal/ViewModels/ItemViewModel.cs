﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;
using PropertyChanged;
using wallabag.Common;
using wallabag.Common.Mvvm;
using wallabag.Models;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.Web.Http;

namespace wallabag.ViewModels
{
    [ImplementPropertyChanged]
    public class ItemViewModel : ViewModelBase
    {
        private static SQLite.SQLiteAsyncConnection conn = new SQLite.SQLiteAsyncConnection(Helpers.DATABASE_PATH);
        public override string ViewModelIdentifier { get; set; } = "ItemViewModel";

        public ItemViewModel(Item Model)
        {
            this.Model = Model;
            DeleteCommand = new Command(async () => await DeleteItemAsync());
            SwitchReadStatusCommand = new Command(async () => await SwitchReadValueAsync());
            SwitchFavoriteStatusCommand = new Command(async () => await SwitchFavoriteValueAsync());

            Model.Tags.CollectionChanged += Tags_CollectionChanged;
        }

        public Item Model { get; set; }
        public string ContentWithHeader { get; set; }
        public string IntroSentence { get; set; }

        public Command DeleteCommand { get; private set; }
        public Command SwitchReadStatusCommand { get; private set; }
        public Command SwitchFavoriteStatusCommand { get; private set; }

        #region Methods
        public async Task CreateContentFromTemplateAsync()
        {
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Article/article.html"));
            string _template = await FileIO.ReadTextAsync(file);

            string accentColor = Application.Current.Resources["SystemAccentColor"].ToString().Remove(1,2);
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
        public void GetHeaderImage()
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(Model.Content);
            if (document.DocumentNode.Descendants("img").Count() > 0)
            {
                foreach (var item in document.DocumentNode.Descendants("img"))
                    if (item.Attributes.Contains("src"))
                    {
                        Model.PreviewPictureUri = item.Attributes["src"].Value;
                        return;
                    }
            }
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

        public async Task<bool> DeleteItemAsync()
        {
            NavigationService.GoBack();
            bool result = Model.IsDeleted = await DeleteItemAsync(Model.Id);
            await conn.UpdateAsync(Model);
            return result;
        }
        public static async Task<bool> DeleteItemAsync(int ItemId, bool IsOfflineAction = false)
        {
            var response = await Helpers.ExecuteHttpRequestAsync(Helpers.HttpRequestMethod.Delete, $"/entries/{ItemId}");

            if (response.StatusCode == HttpStatusCode.Ok)
                return true;
            else
            {
                if (!IsOfflineAction)
                    await conn.InsertAsync(new OfflineAction()
                    {
                        Task = OfflineAction.OfflineActionTask.DeleteItem,
                        ItemId = ItemId
                    });
                return false;
            }
        }
        public async Task<bool> UpdateItemAsync()
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("title", Model.Title);
            parameters.Add("tags", Model.Tags.ToCommaSeparatedString());
            parameters.Add("archive", Model.IsRead);
            parameters.Add("star", Model.IsStarred);
            parameters.Add("delete", Model.IsDeleted);

            var response = await Helpers.ExecuteHttpRequestAsync(Helpers.HttpRequestMethod.Patch, $"/entries/{Model.Id}", parameters);
            if (response.StatusCode == HttpStatusCode.Ok)
            {
                Item resultItem = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<Item>(response.Content.ToString()));
                if (resultItem.Title == Model.Title &&
                    resultItem.IsRead == Model.IsRead &&
                    resultItem.IsStarred == Model.IsStarred &&
                    resultItem.IsDeleted == Model.IsDeleted)
                {
                    Model.LastUpdated = resultItem.LastUpdated;
                    await conn.UpdateAsync(Model);
                    return true;
                }
            }
            return false;
        }
        public static async Task<bool> UpdateSpecificProperty(int itemId, string propertyName, object propertyValue)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add(propertyName, propertyValue);

            var response = await Helpers.ExecuteHttpRequestAsync(Helpers.HttpRequestMethod.Patch, $"/entries/{itemId}", parameters);
            if (response.StatusCode == HttpStatusCode.Ok)
                return true;
            return false;
        }

        public async static Task<ObservableCollection<Tag>> AddTagsAsync(int ItemId, string tags, bool IsOfflineAction = false)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>() {["tags"] = tags };
            var response = await Helpers.ExecuteHttpRequestAsync(Helpers.HttpRequestMethod.Post, $"/entries/{ItemId}/tags", parameters);

            if (response.StatusCode == HttpStatusCode.Ok)
            {
                var json = JsonConvert.DeserializeObject<Item>(response.Content.ToString());
                return json.Tags ?? new ObservableCollection<Tag>();
            }
            else
            {
                if (!IsOfflineAction)
                    await conn.InsertAsync(new OfflineAction()
                    {
                        Task = OfflineAction.OfflineActionTask.AddTags,
                        ItemId = ItemId,
                        TagsString = tags
                    });
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
            var response = await Helpers.ExecuteHttpRequestAsync(Helpers.HttpRequestMethod.Delete,$"/entries/{ItemId}/tags/{TagId}");

            if (response.StatusCode == HttpStatusCode.Ok)
                return true;
            else
            {
                if (!IsOfflineAction)
                    await conn.InsertAsync(new OfflineAction()
                    {
                        Task = OfflineAction.OfflineActionTask.DeleteTag,
                        ItemId = ItemId,
                        TagId = TagId
                    });
                return false;
            }
        }

        public async Task<bool> SwitchReadValueAsync()
        {
            Model.IsRead = !Model.IsRead;

            OfflineAction.OfflineActionTask actionTask = OfflineAction.OfflineActionTask.MarkItemAsRead;

            if (!Model.IsRead)
                actionTask = OfflineAction.OfflineActionTask.UnmarkItemAsRead;

            await conn.InsertAsync(new OfflineAction()
            {
                Task = actionTask,
                ItemId = Model.Id
            });
            return true;
        }
        public async Task<bool> SwitchFavoriteValueAsync()
        {
            Model.IsStarred = !Model.IsStarred;

            OfflineAction.OfflineActionTask actionTask = OfflineAction.OfflineActionTask.MarkItemAsFavorite;

            if (!Model.IsStarred)
                actionTask = OfflineAction.OfflineActionTask.UnmarkItemAsFavorite;

            await conn.InsertAsync(new OfflineAction()
            {
                Task = actionTask,
                ItemId = Model.Id
            });
            return true;
        }
        #endregion
    }
}
