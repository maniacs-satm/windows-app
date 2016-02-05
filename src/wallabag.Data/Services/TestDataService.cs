using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using wallabag.Common;
using wallabag.Data.Interfaces;
using wallabag.Data.Models;
using wallabag.Models;
using Windows.Foundation;

namespace wallabag.Data.Services
{
    public class TestDataService : IDataService
    {
        private ObservableCollection<Item> _Items = new ObservableCollection<Item>();

        public bool CredentialsAreExisting { get; } = true;

        private Item GenerateSampleItem(int Id)
        {
            var content = "<b>This is some test content.</b> ";

            for (int i = 0; i < 25; i++)
                content += "This is some test content. ";

            var result = new Item()
            {
                Url = "https://localhost/" + Id,
                Title = "Sample item #" + Id,
                Content = content,
                PreviewPictureUri = "https://jlnostr.de/content/1-projects/1-wallabag-for-windows/header.png"
            };
            return result;
        }

        public Task<bool> AddItemAsync(string Url, string TagsString = "", string Title = "", bool IsOfflineTask = false)
        {
            var content = "<b>This is some test content.</b> ";

            if (string.IsNullOrWhiteSpace(Title))
                Title = new Uri(Url).Host;

            for (int i = 0; i < 25; i++)
                content += "This is some test content. ";

            _Items.Add(new Item()
            {
                Url = Url,
                Tags = TagsString.ToObservableCollection(),
                Title = Title,
                Content = content,
                PreviewPictureUri = "https://jlnostr.de/content/1-projects/1-wallabag-for-windows/header.png"
            });
            return Task.FromResult(true);
        }

        public IAsyncOperationWithProgress<bool, DownloadProgress> DownloadItemsFromServerAsync(bool DownloadAllItems = false)
        {
            Func<CancellationToken, IProgress<DownloadProgress>, Task<bool>> taskProvider = (token, progress) => _DownloadItemsFromServerAsync(progress, DownloadAllItems);
            return AsyncInfo.Run(taskProvider);
        }
        private Task<bool> _DownloadItemsFromServerAsync(IProgress<DownloadProgress> progress, bool DownloadAllItems)
        {
            var dProgress = new DownloadProgress();
            for (int i = 0; i < 10; i++)
            {
                dProgress.CurrentItemIndex = i;
                progress.Report(dProgress);
                _Items.Add(GenerateSampleItem(i));
            }
            return Task.FromResult(true);
        }

        public Task<Item> GetItemAsync(string Title)
        {
            var item = _Items.Where(i => i.Title == Title).FirstOrDefault();
            return Task.FromResult(item);
        }

        public Task<Item> GetItemAsync(int Id)
        {
            var item = _Items.Where(i => i.Id == Id).FirstOrDefault();
            return Task.FromResult(item);
        }

        public Task<List<Item>> GetItemsAsync(FilterProperties filterProperties)
        {
            // TODO: Implement filtering.
            return Task.FromResult(_Items.ToList());
        }

        public Task<List<OfflineTask>> GetOfflineTasksAsync()
        {
            var result = new List<OfflineTask>();
            for (int i = 0; i < 10; i++)
            {
                // TODO

            });

            return Task.FromResult(result);
        }

        public Task<List<Tag>> GetTagsAsync()
        {
            var result = new List<Tag>();

            for (int i = 0; i < 20; i++)
                result.Add(new Tag()
                {
                    Id = i,
                    Label = "Tag #" + i
                });

            return Task.FromResult(result);
        }

        public Task InitializeDatabaseAsync()
        {
            _Items = new ObservableCollection<Item>();
            return Task.CompletedTask;
        }

        public Task<bool> SyncOfflineTasksWithServerAsync()
        {
            return Task.FromResult(true);
        }

        public Task UpdateItemAsync(Item item)
        {
            _Items.Remove(_Items.Where(i => i.Id == item.Id).FirstOrDefault());
            _Items.Add(item);
            return Task.CompletedTask;
        }

        public Task<bool> LoginAsync(string url, string username, string password)
        {
            return Task.FromResult(true);
        }
    }
}
