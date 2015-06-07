using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using wallabag.DataModel;

namespace wallabag.UnitTests
{
    [TestClass]
    public class ItemViewModelTests
    {
        private ItemViewModel ViewModel;

        [TestInitialize]
        public void Initialize()
        {
            ViewModel = new ItemViewModel(new Item() { Id = 1 });
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
            Assert.AreEqual(true, ViewModel.Model.IsArchived);
            Assert.AreEqual(false, ViewModel.Model.IsStarred);
        }

        [TestMethod]
        public async Task FetchItem()
        {
            Assert.AreEqual(true, await ViewModel.Fetch());
        }

        [TestMethod]
        public async Task GetTags()
        {
            Assert.AreEqual(true, await ViewModel.GetTags());
        }

        [TestMethod]
        public async Task AddTags()
        {
            Assert.AreEqual(true, await ViewModel.AddTags("tag1,tag2,tag3"));
            CollectionAssert.Contains(ViewModel.Model.Tags, "tag1");
        }

        [TestMethod]
        public async Task DeleteOneTag()
        {
            Assert.AreEqual(true, await ViewModel.DeleteTag("tag2"));
            CollectionAssert.DoesNotContain(ViewModel.Model.Tags, "tag2");
        }
    }
}
