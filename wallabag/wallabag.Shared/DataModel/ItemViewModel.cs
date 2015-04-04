using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using wallabag.Common;
using Windows.UI;
using Windows.Web.Http;

namespace wallabag.DataModel
{
    [ImplementPropertyChanged]
    public class ItemViewModel : ViewModelBase
    {
        public Item Model { get; set; }
        public ObservableCollection<string> Tags { get; set; }

        public string ContentWithTitle
        {
            get
            {
                var content =
                "<html><head><link rel=\"stylesheet\" href=\"ms-appx-web:///Assets/css/wallabag.css\" type=\"text/css\" media=\"screen\" />" + GenerateCSS() + "</head>" +
                "<h1 class=\"wallabag-header\">" + Model.Title + "</h1>" +
                Model.Content +
                "</html>";
                return content;
            }
        }
        private string GenerateCSS()
        {
            string css = "body {" +
                CSSproperty("font-size", AppSettings.FontSize + "px") +
                CSSproperty("line-height", AppSettings.LineHeight.ToString().Replace(",", ".")) +
                CSSproperty("color", AppSettings.TextColor) +
                CSSproperty("background", AppSettings.BackgroundColor) +
#if WINDOWS_APP
 CSSproperty("max-width", "960px") +
                CSSproperty("margin", "0 auto") +
                CSSproperty("padding", "0 20px") +
#endif
 "}";
            return "<style>" + css + "</style>";
        }
        private string CSSproperty(string name, object value)
        {
            if (value.GetType() != typeof(Color))
            {
                return string.Format("{0}: {1};", name, value.ToString());
            }
            else
            {
                var color = (Color)value;
                var tmpColor = string.Format("rgba({0}, {1}, {2}, {3})", color.R, color.G, color.B, color.A);
                return string.Format("{0}: {1};", name, tmpColor);
            }
        }

        public ItemViewModel(Item Model)
        {
            this.Model = Model;
        }

        public async Task<bool> Delete()
        {
            HttpClient http = new HttpClient();

            await Helpers.AddHeaders(http);
            var response = await http.DeleteAsync(new Uri(string.Format("http://v2.wallabag.org/api/entries/{0}.json", Model.Id)));
            http.Dispose();

            if (response.StatusCode == HttpStatusCode.Ok)
                return true;
            return false;
        }
        public async Task<bool> Update()
        {
            HttpClient http = new HttpClient();

            await Helpers.AddHeaders(http);

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("title", Model.Title);
            //parameters.Add("tags", "tags comma-separated"); TODO
            parameters.Add("archive", Model.IsArchived);
            parameters.Add("star", Model.IsStarred);
            parameters.Add("delete", Model.IsDeleted);

            var content = new HttpStringContent(JsonConvert.SerializeObject(parameters));
            var response = await http.PatchAsync(new Uri(string.Format("http://v2.wallabag.org/api/entries/{0}.json", Model.Id)), content);
            http.Dispose();

            if (response.StatusCode == HttpStatusCode.Ok)
                return true;
            return false;
        }
        public async Task<bool> Fetch()
        {
            HttpClient http = new HttpClient();

            await Helpers.AddHeaders(http);
            var response = await http.GetAsync(new Uri(string.Format("http://v2.wallabag.org/api/entries/{0}.json", Model.Id)));
            http.Dispose();

            if (response.StatusCode == HttpStatusCode.Ok)
            {
                this.Model = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<Item>(response.Content.ToString()));
                return true;
            }
            return false;
        }

        public async Task GetTags()
        {
            HttpClient http = new HttpClient();

            await Helpers.AddHeaders(http);
            var response = await http.GetAsync(new Uri(string.Format("http://v2.wallabag.org/api/entries/{0}/tags.json", Model.Id)));
            http.Dispose();

            if (response.StatusCode != HttpStatusCode.NoContent &&
                response.StatusCode == HttpStatusCode.Ok)
            {
                //TODO: JSON parsing
            }
        }
        public async Task<bool> AddTags(string tags)
        {
            HttpClient http = new HttpClient();

            await Helpers.AddHeaders(http);
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("tags", tags);

            var content = new HttpStringContent(JsonConvert.SerializeObject(parameters));
            var response = await http.PostAsync(new Uri(string.Format("http://v2.wallabag.org/api/entries/{0}/tags.json", Model.Id)), content);
            http.Dispose();

            if (response.StatusCode == HttpStatusCode.Ok)
            {
                string[] tagarray = tags.Split(",".ToCharArray());

                foreach (string tag in tagarray)
                    Tags.Add(tag);

                return true;
            }
            return false;
        }
        public async Task<bool> DeleteTag(string tag)
        {
            HttpClient http = new HttpClient();

            await Helpers.AddHeaders(http);
            var response = await http.DeleteAsync(new Uri(string.Format("http://v2.wallabag.org/api/entries/{0}/tags/{1}.json", Model.Id, tag)));
            http.Dispose();

            if (response.StatusCode == HttpStatusCode.Ok)
                return true;
            return false;
        }
    }
}
