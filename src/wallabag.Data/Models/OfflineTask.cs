using GalaSoft.MvvmLight.Messaging;
using SQLite;
using System;
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

        public OfflineTaskAction Action { get; set; }
        public int ItemId { get; set; }
        public string ItemTitle { get; set; }

        public string RequestUri { get; set; }
        public Dictionary<string, object> RequestParameters { get; set; }
        public HttpRequestMethod RequestMethod { get; set; }

        public OfflineTask()
        {
            RequestUri = string.Empty;
            RequestParameters = new Dictionary<string, object>();
            RequestMethod = HttpRequestMethod.Patch;
            Action = OfflineTaskAction.MarkAsRead;
            ItemId = -1;
            ItemTitle = string.Empty;
        }

        public enum OfflineTaskAction
        {
            MarkAsRead = 0,
            UnmarkAsRead = 1,
            MarkAsFavorite = 2,
            UnmarkAsFavorite = 3,
            AddTags = 4,
            DeleteTag = 5,
            Delete = 6,
            AddItem = 7
        }

        public static string ItemReadAPIString { get; } = "archive";
        public static string ItemStarredAPIString { get; } = "star";

        public static Task AddToQueueAsync(Item Item, OfflineTaskAction action, object parameter = null)
        {
            var requestUri = $"/entries/{Item.Id}";
            var parameterDictionary = new Dictionary<string, object>();
            var method = HttpRequestMethod.Patch;

            switch (action)
            {
                case OfflineTaskAction.MarkAsRead:
                    parameterDictionary.Add(ItemReadAPIString, true);
                    break;
                case OfflineTaskAction.UnmarkAsRead:
                    parameterDictionary.Add(ItemReadAPIString, false);
                    break;
                case OfflineTaskAction.MarkAsFavorite:
                    parameterDictionary.Add(ItemStarredAPIString, true);
                    break;
                case OfflineTaskAction.UnmarkAsFavorite:
                    parameterDictionary.Add(ItemStarredAPIString, false);
                    break;
                case OfflineTaskAction.AddTags:
                    if (parameter.GetType() != typeof(string))
                        throw new InvalidOperationException("To add some tags, these must be submitted as comma-separated list.");

                    requestUri = $"/entries/{Item.Id}/tags";
                    parameterDictionary.Add("tags", parameter);
                    method = HttpRequestMethod.Post;
                    break;
                case OfflineTaskAction.DeleteTag:
                    if (parameter.GetType() != typeof(Tag))
                        throw new InvalidOperationException("To delete a tag the parameter must be of type 'Tag'.");

                    requestUri = $"/entries/{Item.Id}/tags/{(parameter as Tag).Id}";
                    method = HttpRequestMethod.Delete;
                    break;
                case OfflineTaskAction.Delete:
                    method = HttpRequestMethod.Delete;
                    break;
                case OfflineTaskAction.AddItem:
                    if (parameter.GetType() != typeof(Dictionary<string, object>))
                        throw new InvalidOperationException("To add an item, submit a Dictionary<string, object> with values for 'url','tags' and 'title'.");

                    requestUri = "/entries";
                    parameterDictionary = parameter as Dictionary<string, object>;
                    method = HttpRequestMethod.Post;
                    break;
                default:
                    break;
            }

            Dictionary<string, object> parameterToSubmit = null;
            if (parameterDictionary.Count > 0)
                parameterToSubmit = parameterDictionary;

            return AddToQueueAsync(Item, action, requestUri, parameterToSubmit, method);
        }
        private static async Task AddToQueueAsync(Item Item, OfflineTaskAction action, string requestUri, Dictionary<string, object> parameters, HttpRequestMethod method = HttpRequestMethod.Patch)
        {
            var newTask = new OfflineTask();

            newTask.RequestUri = requestUri;
            newTask.RequestParameters = parameters;
            newTask.RequestMethod = method;
            newTask.Action = action;
            newTask.ItemId = Item.Id;
            newTask.ItemTitle = Item.Title;

            await new SQLiteAsyncConnection(DATABASE_PATH).InsertAsync(newTask);
            Messenger.Default.Send(new NotificationMessage("UpdateQueue"));
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

        internal Task DeleteFromQueueAsync() => new SQLiteAsyncConnection(DATABASE_PATH).DeleteAsync(this);

    }
}
