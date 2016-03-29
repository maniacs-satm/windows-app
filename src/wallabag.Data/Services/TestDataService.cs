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
                Id = Id,
                Url = "https://localhost/" + Id,
                Title = "Sample item #" + Id,
                IsRead = Id % 2 != 0,
                IsStarred = Id % 2 == 0,
                Content = content,
                DomainName = "localhost",
                PreviewPictureUri = "http://lorempixel.com/300/200?=" + Id
            };
            return result;
        }

        public Task<bool> AddItemAsync(string Url, string TagsString = "", string Title = "", bool IsOfflineTask = false)
        {
            var newItem = GenerateSampleItem(-1);
            newItem.Url = Url;
            newItem.Tags = TagsString.ToObservableCollection();

            if (string.IsNullOrWhiteSpace(Title))
                newItem.Title = new Uri(Url).Host;

            _Items.Add(newItem);
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
            var maximum = _Items.Count + 10;
            for (int i = _Items.Count; i < maximum; i++)
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
            var result = _Items.ToList();

            switch (filterProperties.ItemType)
            {
                case FilterProperties.FilterPropertiesItemType.Unread:
                    result.Replace(result.Where(i => i.IsRead == false).ToList());
                    break;
                case FilterProperties.FilterPropertiesItemType.Favorites:
                    result.Replace(result.Where(i => i.IsStarred == true).ToList());
                    break;
                case FilterProperties.FilterPropertiesItemType.Archived:
                    result.Replace(result.Where(i => i.IsRead == true).ToList());
                    break;
                case FilterProperties.FilterPropertiesItemType.Deleted:
                    result.Replace(result.Where(i => i.IsDeleted == true).ToList());
                    break;
                default:
                    break;
            }

            if (!string.IsNullOrWhiteSpace(filterProperties.SearchQuery))
                result = result.Where(i => i.Title.Contains(filterProperties.SearchQuery)).ToList(); ;

            return Task.FromResult(result);
        }

        public async Task<List<OfflineTask>> GetOfflineTasksAsync()
        {
            var result = new List<OfflineTask>();
            for (int i = 0; i < 10; i++)
            {
                result.Add(new OfflineTask()
                {
                    Action = await GetRandomOfflineTaskAction(),
                    Id = i,
                    ItemId = i
                });
            }

            return result;
        }

        private async Task<OfflineTask.OfflineTaskAction> GetRandomOfflineTaskAction()
        {
            Array values = Enum.GetValues(typeof(OfflineTask.OfflineTaskAction));
            Random random = new Random();

            await Task.Delay(TimeSpan.FromMilliseconds(10));
            return (OfflineTask.OfflineTaskAction)values.GetValue(random.Next(values.Length));
        }

        public Task<List<Tag>> GetTagsAsync()
        {
            var result = new List<Tag>();

            for (int i = 0; i < 15; i++)
                result.Add(new Tag()
                {
                    Id = i,
                    Label = "Tag #" + i
                });

            return Task.FromResult(result);
        }

        public async Task InitializeDatabaseAsync()
        {
            _Items = new ObservableCollection<Item>();
            await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync(Helpers.DATABASE_FILENAME, Windows.Storage.CreationCollisionOption.ReplaceExisting);
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

        public Task DeleteItemAsync(Item item)
        {
            _Items.Remove(item);
            return Task.CompletedTask;
        }
    }
}
