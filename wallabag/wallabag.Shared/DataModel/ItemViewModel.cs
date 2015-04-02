using System;
using System.Collections.Generic;
using System.Text;
using PropertyChanged;
using Newtonsoft.Json;
using System.Threading.Tasks;
using wallabag.Common;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

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
            string response = string.Empty;
            using (HttpClient http = new HttpClient())
            {
                await Helpers.AddHeaders(http, Model.User);
                response = await http.GetStringAsync(new Uri(string.Format("http://v2.wallabag.org/api/entries/{0}/tags.json", Model.Id)));
            }
            if (!String.IsNullOrWhiteSpace(response))
            {
                // TODO: JSON parsing
            }
        }
    }
}
