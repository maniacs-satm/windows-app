using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PropertyChanged;
using wallabag.Common;
using wallabag.Common.MVVM;
using Windows.UI;
using Windows.Web.Http;

namespace wallabag.DataModel
{
    [ImplementPropertyChanged]
    [DataContract]
    public class ItemViewModel : ViewModelBase
    {
        #region Properties
        [DataMember]
        public Item Model { get; set; }
        [DataMember]
        public bool IsSelected { get; set; }

        public string ContentWithHeader
        {
            get
            {
                var content =
                $"<html><head><link rel=\"stylesheet\" href=\"ms-appx-web:///Assets/wallabag.css\" type=\"text/css\" media=\"screen\" />{GenerateCSS()}</head>" +
                $"<h1 class=\"wallabag-header\">{Model.Title}</h1>" +
                $"<h2 class=\"wallabag-subheader\"><a href=\"{Model.Url}\">{UrlHostname}</a></h2>" +
                Model.Content +
                "</html>";
                return content;
            }
        }
        public string UrlHostname
        {
            get
            {
                try { return new Uri(Model.Url).Host; }
                catch { return string.Empty; }
            }
        }

        private string GenerateCSS()
        {
            string css = "body {" +
                CSSproperty("font-size", AppSettings.FontSize + "px") +
                CSSproperty("line-height", AppSettings.LineHeight.ToString().Replace(",", ".")) +
                CSSproperty("color", AppSettings.TextColor) +
                CSSproperty("background", AppSettings.BackgroundColor) +
                CSSproperty("max-width", "960px") +
                CSSproperty("margin", "0 auto") +
                CSSproperty("padding", "0 20px") +
                "}";
            return "<style>" + css + "</style>";
        }
        private string CSSproperty(string name, object value)
        {
            if (value.GetType() != typeof(Color))
            {
                return $"{name}: {value.ToString()};";
            }
            else
            {
                var color = (Color)value;
                var tmpColor = $"rgba({color.R}, {color.G}, {color.B}, {color.A})";
                return $"{name}: {tmpColor};";
            }
        }
        private string CreateStringOfTagList()
        {
            string result = string.Empty;
            if (Model.Tags != null && Model.Tags.Count > 0)
            {
                foreach (Tag tag in Model.Tags)
                    result += tag + ",";

                result = result.Remove(result.Length - 1, 1); // Remove the last comma.
            }
            return result;
        }
        #endregion

        public ItemViewModel(Item Model)
        {
            this.Model = Model;
            UpdateCommand = new RelayCommand(async () => await Update());
            DeleteCommand = new RelayCommand(async () => await Delete());
            AddTagsCommand = new RelayCommand<string>(async (t) => await AddTags((string)t));
            SwitchReadStatusCommand = new RelayCommand(async () => await SwitchReadStatus());
            SwitchFavoriteStatusCommand = new RelayCommand(async () => await SwitchFavoriteStatus());
        }

        public RelayCommand UpdateCommand { get; private set; }
        public RelayCommand DeleteCommand { get; private set; }
        public RelayCommand<string> AddTagsCommand { get; private set; }
        public RelayCommand SwitchReadStatusCommand { get; private set; }
        public RelayCommand SwitchFavoriteStatusCommand { get; private set; }

        #region Methods
        public async Task<bool> Delete()
        {
            HttpClient http = new HttpClient();

            await Helpers.AddHeaders(http);
            var response = await http.DeleteAsync(new Uri($"{AppSettings.wallabagUrl}/api/entries/{Model.Id}.json"));
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
            parameters.Add("tags", CreateStringOfTagList());
            parameters.Add("archive", Model.IsArchived);
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
                        resultItem.IsArchived == Model.IsArchived &&
                        resultItem.IsStarred == Model.IsStarred &&
                        resultItem.IsDeleted == Model.IsDeleted)
                    {
                        Model.UpdatedAt = resultItem.UpdatedAt;
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
                var response = await http.GetAsync(new Uri($"{AppSettings.wallabagUrl}/api/entries/{Model.Id}.json"));
                http.Dispose();

                if (response.StatusCode == HttpStatusCode.Ok)
                {
                    this.Model = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<Item>(response.Content.ToString()));
                    return true;
                }
            }
            catch { return false; }
            return false;
        }

        public async Task GetTags()
        {
            HttpClient http = new HttpClient();

            await Helpers.AddHeaders(http);
            try
            {
                var response = await http.GetAsync(new Uri($"{AppSettings.wallabagUrl}/api/entries/{Model.Id}/tags.json"));
                http.Dispose();

                if (response.StatusCode != HttpStatusCode.NoContent &&
                    response.StatusCode == HttpStatusCode.Ok)
                {
                    Model.Tags = JsonConvert.DeserializeObject<List<Tag>>(await response.Content.ReadAsStringAsync());
                }
            }
            catch { }
        }
        public async Task<bool> AddTags(string tags)
        {
            HttpClient http = new HttpClient();

            await Helpers.AddHeaders(http);
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("tags", tags);

            var content = new HttpStringContent(JsonConvert.SerializeObject(parameters), Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json");
            try
            {
                var response = await http.PostAsync(new Uri($"{AppSettings.wallabagUrl}/api/entries/{Model.Id}/tags.json"), content);
                http.Dispose();

                if (response.StatusCode == HttpStatusCode.Ok)
                {
                    string[] tagarray = tags.Split(",".ToCharArray());

                    foreach (string tag in tagarray)
                        Model.Tags.Add(new Tag() { Label = tag });

                    return true;
                }
            }
            catch { return false; }
            return false;
        }
        public async Task<bool> DeleteTag(string tag)
        {
            HttpClient http = new HttpClient();

            await Helpers.AddHeaders(http);
            try
            {
                var response = await http.DeleteAsync(new Uri($"{AppSettings.wallabagUrl}/api/entries/{Model.Id}/tags/{tag}.json"));
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
