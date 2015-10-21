using System.Collections.Generic;
using System.Threading.Tasks;
using SQLite;
using Windows.Web.Http;
using static wallabag.Common.Helpers;

namespace wallabag.Models
{
    [Table("OfflineTasks")]
    public class OfflineTask
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string RequestUri { get; set; }
        public Dictionary<string, object> RequestParameters { get; set; }
        public HttpRequestMethod RequestMethod { get; set; }

        public async Task<bool> ExecuteAsync()
        {
            var response = await ExecuteHttpRequestAsync(RequestMethod, RequestUri, RequestParameters);
            if (response.StatusCode == HttpStatusCode.Ok)
                return true;
            else
                return false;
        }

        public OfflineTask() { }
        public OfflineTask(string requestUri, Dictionary<string, object> parameters, HttpRequestMethod method = HttpRequestMethod.Patch)
        {
            RequestUri = requestUri;
            RequestParameters = parameters;
            RequestMethod = method;
        }
    }
}
