using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PropertyChanged;
using wallabag.Common;
using wallabag.Common.Mvvm;
using wallabag.Models;
using Windows.Storage;
using Windows.Web.Http;

namespace wallabag.ViewModels
{
    [ImplementPropertyChanged]
    public class ItemViewModel : ViewModelBase
    {
        public override string ViewModelIdentifier { get; set; } = "ItemViewModel";

        #region Properties
        public Item Model { get; set; }

        public string ContentWithHeader { get; set; }
        public string UrlHostname
        {
            get
            {
                try { return new Uri(Model.Url).Host.Replace("www.", ""); }
                catch { return string.Empty; }
            }
        }
        #endregion

        public ItemViewModel(Item Model)
        {
            this.Model = Model;
            UpdateCommand = new Command(async () => await UpdateItemAsync());
            DeleteCommand = new Command(async () => await DeleteItemAsync());
            SwitchReadStatusCommand = new Command(async () => await SwitchReadValueAsync());
            SwitchFavoriteStatusCommand = new Command(async () => await SwitchFavoriteValueAsync());

            Model.Tags.CollectionChanged += Tags_CollectionChanged;
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

        public Command UpdateCommand { get; private set; }
        public Command DeleteCommand { get; private set; }
        public Command SwitchReadStatusCommand { get; private set; }
        public Command SwitchFavoriteStatusCommand { get; private set; }

        #region Methods

        public async Task CreateContentFromTemplateAsync()
        {
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Article/article.html"));
            string _template = await FileIO.ReadTextAsync(file);

            ContentWithHeader = _template.FormatWith(new
            {
                title = Model.Title,
                content = Model.Content,
                articleUrl = Model.Url,
                hostname = UrlHostname,
                color = AppSettings.ColorScheme,
                font = AppSettings.FontFamily,
                fontSize = AppSettings.FontSize,
                lineHeight = AppSettings.LineHeight,
                progress = Model.ReadingProgress
            });
        }

        public async Task<bool> DeleteItemAsync()
        {
            return Model.IsDeleted = await DeleteItemAsync(Model.Id);
        }
        public static async Task<bool> DeleteItemAsync(int ItemId)
        {
            HttpClient http = new HttpClient();

            await Helpers.AddHttpHeadersAsync(http);
            var response = await http.DeleteAsync(new Uri($"{AppSettings.wallabagUrl}/api/entries/{ItemId}.json"));
            http.Dispose();

            if (response.StatusCode == HttpStatusCode.Ok)
                return true;
            return false;
        }
        public async Task<bool> UpdateItemAsync()
        {
            HttpClient http = new HttpClient();

            await Helpers.AddHttpHeadersAsync(http);

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("title", Model.Title);
            parameters.Add("tags", Model.Tags.ToCommaSeparatedString());
            parameters.Add("archive", Model.IsRead);
            parameters.Add("star", Model.IsStarred);
            parameters.Add("delete", Model.IsDeleted);

            var content = new HttpStringContent(JsonConvert.SerializeObject(parameters), Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json");
            try
            {
                var response = await http.PatchAsync(new Uri($"{AppSettings.wallabagUrl}/api/entries/{Model.Id}.json"), content);
                http.Dispose();

                if (response.StatusCode == HttpStatusCode.Ok)
                {
                    Item resultItem = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<Item>(response.Content.ToString()));
                    if (resultItem.Title == Model.Title &&
                        resultItem.IsRead == Model.IsRead &&
                        resultItem.IsStarred == Model.IsStarred &&
                        resultItem.IsDeleted == Model.IsDeleted)
                    {
                        Model.LastUpdated = resultItem.LastUpdated;
                        return true;
                    }
                }
            }
            catch { return false; }
            return false;
        }
        public async Task<bool> FetchInformationForItemAsync()
        {
            HttpClient http = new HttpClient();

            await Helpers.AddHttpHeadersAsync(http);
            try
            {
                var response = await http.GetAsync(new Uri($"{AppSettings.wallabagUrl}/api/entries/{Model.Id}.json"));
                http.Dispose();

                if (response.StatusCode == HttpStatusCode.Ok)
                {
                    Model = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<Item>(response.Content.ToString()));
                    return true;
                }
            }
            catch { return false; }
            return false;
        }

        public async static Task<ObservableCollection<Tag>> AddTagsAsync(int ItemId, string tags)
        {
            HttpClient http = new HttpClient();

            await Helpers.AddHttpHeadersAsync(http);
            Dictionary<string, object> parameters = new Dictionary<string, object>() {["tags"] = tags };

            var content = new HttpStringContent(JsonConvert.SerializeObject(parameters), Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json");
            try
            {
                var response = await http.PostAsync(new Uri($"{AppSettings.wallabagUrl}/api/entries/{ItemId}/tags.json"), content);
                http.Dispose();

                if (response.StatusCode == HttpStatusCode.Ok)
                {
                    var json = JsonConvert.DeserializeObject<Item>(response.Content.ToString());
                    return json.Tags ?? new ObservableCollection<Tag>();
                }
            }
            catch
            {
                return null;
            }
            return null;
        }
        public async Task<bool> DeleteTagAsync(Tag Tag)
        {
            return await DeleteTagAsync(Model.Id, Tag.Id);
        }
        public async static Task<bool> DeleteTagAsync(int ItemId, int TagId)
        {
            HttpClient http = new HttpClient();

            await Helpers.AddHttpHeadersAsync(http);
            try
            {
                var response = await http.DeleteAsync(new Uri($"{AppSettings.wallabagUrl}/api/entries/{ItemId}/tags/{TagId}.json"));
                http.Dispose();

                if (response.StatusCode == HttpStatusCode.Ok)
                    return true;
            }
            catch { return false; }
            return false;
        }

        public async Task<bool> SwitchReadValueAsync()
        {
            if (Model.IsRead)
                Model.IsRead = false;
            else
                Model.IsRead = true;
            return await UpdateItemAsync();
        }
        public async Task<bool> SwitchFavoriteValueAsync()
        {
            if (Model.IsStarred)
                Model.IsStarred = false;
            else
                Model.IsStarred = true;
            return await UpdateItemAsync();
        }
        #endregion
    }
}
