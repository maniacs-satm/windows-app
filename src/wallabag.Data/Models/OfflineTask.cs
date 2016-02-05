using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;
using wallabag.Models;
using Windows.Web.Http;
using static wallabag.Common.Helpers;

namespace wallabag.Data.Models
{
    [Table("OfflineTasks")]
    public class OfflineTask
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int ActionId { get; set; }

        public string RequestUri { get; set; }
        public Dictionary<string, object> RequestParameters { get; set; }
        public HttpRequestMethod RequestMethod { get; set; }

        public OfflineTask() {
            RequestUri = string.Empty;
            RequestParameters = new Dictionary<string, object>();
            RequestMethod = HttpRequestMethod.Patch;
        }

        public enum Action
        {
            MarkAsRead = 0,
            UnmarkAsRead = 1,
            MarkAsFavorite = 2,
            UnmarkAsFavorite = 3,
            ModifyTags = 4,
            Delete = 5,
            AddItem = 6
        }

        public static async Task AddTaskAsync(Item Item, Action action, string requestUri, Dictionary<string, object> parameters, HttpRequestMethod method = HttpRequestMethod.Patch)
        {
            var newTask = new OfflineTask();

            newTask.RequestUri = requestUri;
            newTask.RequestParameters = parameters;
            newTask.RequestMethod = method;
            newTask.ActionId = action.GetHashCode();

            await new SQLiteAsyncConnection(DATABASE_PATH).InsertAsync(newTask);
        }
        public async Task<bool> ExecuteAsync()
        {
            var response = await ExecuteHttpRequestAsync(RequestMethod, RequestUri, RequestParameters);
            if (response.StatusCode == HttpStatusCode.Ok ||
                response.StatusCode == HttpStatusCode.NotFound)
            {
                await new SQLiteAsyncConnection(DATABASE_PATH).DeleteAsync(this);
                return true;
            }
            else
                return false;
        }
    }
}
