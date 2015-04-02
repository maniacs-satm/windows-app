using PropertyChanged;
using System;
using System.Threading.Tasks;
using wallabag.Common;
using Windows.Web.Http;

namespace wallabag.DataModel
{
    [ImplementPropertyChanged]
    public class ItemViewModel
    {
        public ItemViewModel(Item Model)
        {
            this.Model = Model;
        }
        public Item Model { get; set; }

        public async Task GetTags()
        {
            HttpClient http = new HttpClient();

            await Helpers.AddHeaders(http, Model.User);
            System.Diagnostics.Debug.WriteLine(http.DefaultRequestHeaders["X-WSSE"]);
            var response = await http.GetAsync(new Uri(string.Format("http://v2.wallabag.org/api/entries/{0}/tags.json", Model.Id)));
            http.Dispose();

            if (response.StatusCode != HttpStatusCode.NoContent ||
                !response.IsSuccessStatusCode)
            {
                //TODO: JSON parsing
            }
        }
    }
}
