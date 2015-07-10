using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PropertyChanged;
using wallabag.Common;
using wallabag.Common.Mvvm;
using Windows.Storage;
using Windows.Web.Http;

namespace wallabag.DataModel
{
    [ImplementPropertyChanged]
    public class ItemViewModel : ViewModelBase
    {
        public override string ViewModelIdentifier { get; set; } = "ItemViewModel";


        #region Properties
        public Item Model { get; set; }
        public ObservableCollection<Tag> Tags { get; set; }

        public string ContentWithHeader { get; set; }
        public string UrlHostname
        {
            get
            {
                try { return new Uri(Model.Url).Host.Replace("www.", ""); }
                catch { return string.Empty; }
            }
        }

        public async Task CreateContentFromTemplate()
        {
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Article/article.html"));
            string _template = await FileIO.ReadTextAsync(file);

            ContentWithHeader = _template.FormatWith(new
            {
                title = Model.Title,
                content = Model.Content,
                articleUrl = Model.Url,
                hostname = UrlHostname
            });
        }
        #endregion

        public ItemViewModel(Item Model)
        {
            this.Model = Model;
            UpdateCommand = new Command(async () => await Update());
            DeleteCommand = new Command(async () => await Delete());
            SwitchReadStatusCommand = new Command(async () => await SwitchReadStatus());
            SwitchFavoriteStatusCommand = new Command(async () => await SwitchFavoriteStatus());

            Tags = Model.TagsString.ToObservableCollection();
            Tags.CollectionChanged += Tags_CollectionChanged;
        }

        private async void Tags_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                foreach (Tag item in e.OldItems)
                    await DeleteTag(item);

            if (e.NewItems != null)
                foreach (Tag item in e.NewItems)
                    await AddTags(item.Label);
        }

        public Command UpdateCommand { get; private set; }
        public Command DeleteCommand { get; private set; }
        public Command SwitchReadStatusCommand { get; private set; }
        public Command SwitchFavoriteStatusCommand { get; private set; }

              #region Methods
        public async Task<bool> Delete()
        {
            HttpClient http = new HttpClient();

            await Helpers.AddHeaders(http);
            var response = await http.DeleteAsync(new Uri($"{AppSettings.Instance.wallabagUrl}/api/entries/{Model.Id}.json"));
            http.Dispose();

            if (response.StatusCode == HttpStatusCode.Ok)
            {
                Model.IsDeleted = true;
                return true;
            }
            return false;
        }
        public async Task<bool> Update()
        {
            HttpClient http = new HttpClient();

            await Helpers.AddHeaders(http);

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("title", Model.Title);
            parameters.Add("tags", Model.TagsString);
            parameters.Add("archive", Model.IsArchived);
            parameters.Add("star", Model.IsStarred);
            parameters.Add("delete", Model.IsDeleted);

            var content = new HttpStringContent(JsonConvert.SerializeObject(parameters), Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json");
            try
            {
                var response = await http.PatchAsync(new Uri($"{AppSettings.Instance.wallabagUrl}/api/entries/{Model.Id}.json"), content);
                http.Dispose();

                if (response.StatusCode == HttpStatusCode.Ok)
                {
                    Item resultItem = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<Item>(response.Content.ToString()));
                    if (resultItem.Title == Model.Title &&
                        resultItem.IsArchived == Model.IsArchived &&
                        resultItem.IsStarred == Model.IsStarred &&
                        resultItem.IsDeleted == Model.IsDeleted)
                    {
                        Model.UpdatedAt = resultItem.UpdatedAt;
                        Model.TagsString = Model.Tags.ToCommaSeparatedString();
                        return true;
                    }
                }
            }
            catch { return false; }
            return false;
        }
        public async Task<bool> Fetch()
        {
            HttpClient http = new HttpClient();

            await Helpers.AddHeaders(http);
            try
            {
                var response = await http.GetAsync(new Uri($"{AppSettings.Instance.wallabagUrl}/api/entries/{Model.Id}.json"));
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

        public async Task<bool> AddTags(string tags)
        {
            HttpClient http = new HttpClient();

            await Helpers.AddHeaders(http);
            Dictionary<string, object> parameters = new Dictionary<string, object>() {["tags"] = tags };

            var content = new HttpStringContent(JsonConvert.SerializeObject(parameters), Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json");
            try
            {
                var response = await http.PostAsync(new Uri($"{AppSettings.Instance.wallabagUrl}/api/entries/{Model.Id}/tags.json"), content);
                http.Dispose();

                if (response.StatusCode == HttpStatusCode.Ok)
                {
                    var json = JsonConvert.DeserializeObject<Item>(response.Content.ToString());
                    foreach (Tag tag in Tags)
                        tag.Id = json.Tags.Where(t => t.Label == tag.Label).FirstOrDefault().Id;

                    return true;
                }
            }
            catch { return false; }
            return false;
        }
        public async Task<bool> DeleteTag(Tag tag)
        {
            HttpClient http = new HttpClient();

            await Helpers.AddHeaders(http);
            try
            {
                var response = await http.DeleteAsync(new Uri($"{AppSettings.Instance.wallabagUrl}/api/entries/{Model.Id}/tags/{tag.Id}.json"));
                http.Dispose();

                if (response.StatusCode == HttpStatusCode.Ok)
                    return true;
            }
            catch { return false; }
            return false;
        }

        public async Task<bool> SwitchReadStatus()
        {
            if (Model.IsArchived)
                Model.IsArchived = false;
            else
                Model.IsArchived = true;
            return await Update();
        }
        public async Task<bool> SwitchFavoriteStatus()
        {
            if (Model.IsStarred)
                Model.IsStarred = false;
            else
                Model.IsStarred = true;
            return await Update();
        }
        #endregion
    }
}
