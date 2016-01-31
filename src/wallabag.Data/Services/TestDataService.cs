using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wallabag.Data.Interfaces;
using wallabag.Models;
using wallabag.Services;
using Windows.Foundation;

namespace wallabag.Data.Services
{
    public class TestDataService : IDataService
    {
        public Task<bool> AddItemAsync(string Url, string TagsString = "", string Title = "", bool IsOfflineTask = false)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperationWithProgress<bool, DownloadProgress> DownloadItemsFromServerAsync(bool DownloadAllItems = false)
        {
            throw new NotImplementedException();
        }

        public Task<Item> GetItemAsync(string Title)
        {
            throw new NotImplementedException();
        }

        public Task<Item> GetItemAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public Task<List<Item>> GetItemsAsync(FilterProperties filterProperties)
        {
            throw new NotImplementedException();
        }

        public Task<List<Tag>> GetTagsAsync()
        {
            throw new NotImplementedException();
        }

        public Task InitializeDatabaseAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> SyncOfflineTasksWithServerAsync()
        {
            throw new NotImplementedException();
        }
    }
}
