using System.Collections.Generic;
using SQLite;

namespace wallabag.Models
{
    [Table("OfflineTasks")]
    public class OfflineTask
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string RequestUri { get; set; }
        public Dictionary<string, object> RequestParameters { get; set; }
        public Common.Helpers.HttpRequestMethod RequestMethod { get; set; }

        public OfflineTask() { }
        public OfflineTask(string requestUri, Dictionary<string, object> parameters, Common.Helpers.HttpRequestMethod method = Common.Helpers.HttpRequestMethod.Patch)
        {
            RequestUri = requestUri;
            RequestParameters = parameters;
            RequestMethod = method;
        }
    }
}
