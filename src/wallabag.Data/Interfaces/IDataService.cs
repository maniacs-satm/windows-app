using System.Collections.Generic;
using System.Threading.Tasks;
using wallabag.Data.Models;
using wallabag.Models;
using Windows.Foundation;

namespace wallabag.Data.Interfaces
{
    public interface IDataService
    {
        Task InitializeDatabaseAsync();

        Task<bool> LoginAsync(string url, string username, string password);
        bool CredentialsAreExisting { get; }

        Task<bool> SyncOfflineTasksWithServerAsync();
        IAsyncOperationWithProgress<bool, DownloadProgress> DownloadItemsFromServerAsync(bool DownloadAllItems = false);

        Task<List<Item>> GetItemsAsync(FilterProperties filterProperties);
        Task<List<Tag>> GetTagsAsync();
        Task<Item> GetItemAsync(int Id);
        Task<Item> GetItemAsync(string Title);

        Task<bool> AddItemAsync(string Url, string TagsString = "", string Title = "", bool IsOfflineTask = false);
        Task UpdateItemAsync(Item item);

        Task<int> GetNumberOfOfflineTasksAsync();
    }
}
