using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using wallabag.DataModel;
using System.Threading.Tasks;

namespace wallabag.Tests
{
    [TestClass]
    public class ItemViewModelTests
    {
        private ItemViewModel ViewModel;

        [TestInitialize]
        public void Initialize()
        {
            ViewModel = new ItemViewModel(new Item()
            {
                Id = 13,
                User = new User() { Username = "wallabag", Password = "wallabag" }
            });
        }

        [TestMethod]
        public async Task DeleteItem()
        {
            Assert.AreEqual(true, await ViewModel.Delete());
        }

        [TestMethod]
        public async Task UpdateItem()
        {
            ViewModel.Model.Title = "Test title change from API";
            ViewModel.Model.IsArchived = false;
            ViewModel.Model.IsStarred = true;
            Assert.AreEqual(true, await ViewModel.Update());
        }

        [TestMethod]
        public async Task FetchItem()
        {
            Assert.AreEqual(true, await ViewModel.Fetch());
            Assert.AreEqual("http://www.glazman.org/weblog/dotclear/index.php?post/2015/01/21/Jean-Claude-Bellamy", ViewModel.Model.Url);
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
