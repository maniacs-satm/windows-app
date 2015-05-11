using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using wallabag.DataModel;
using System.Threading.Tasks;

namespace wallabag.UnitTests
{
    [TestClass]
    public class ItemViewModelTests
    {
        private ItemViewModel ViewModel;

        [TestInitialize]
        public void Initialize()
        {
            ViewModel = new ItemViewModel(new Item() { Id = 38 });
        }

        [TestMethod]
        public async Task DeleteItem()
        {
            Assert.AreEqual(true, await ViewModel.Delete());
        }

        [TestMethod]
        public async Task UpdateItem()
        {
            ViewModel.Model.Title = "Title change from API at " + DateTime.Now;
            ViewModel.Model.IsArchived = true;
            ViewModel.Model.IsStarred = false;
            Assert.AreEqual(true, await ViewModel.Update());
        }

        [TestMethod]
        public async Task FetchItem()
        {
            Assert.AreEqual(true, await ViewModel.Fetch());
        }

        [TestMethod]
        [Ignore]
        public async Task GetTags()
        {
            await ViewModel.GetTags();
        }

        [TestMethod]
        [Ignore]
        public async Task AddTags()
        {
            Assert.AreEqual(true, await ViewModel.AddTags("tag1,tag2,tag3"));
            CollectionAssert.Contains(ViewModel.Tags, "tag1");
        }

        [TestMethod]
        [Ignore]
        public async Task DeleteOneTag()
        {
            Assert.AreEqual(true, await ViewModel.DeleteTag("tag2"));
            CollectionAssert.DoesNotContain(ViewModel.Tags, "tag2");
        }
    }
}
