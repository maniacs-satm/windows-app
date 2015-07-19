using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using wallabag.ViewModels;

namespace wallabag.UnitTests
{
    [TestClass]
    public class ItemViewModelTests
    {
        private ItemViewModel ViewModel;

        [TestInitialize]
        public void Initialize()
        {
            ViewModel = new ItemViewModel(new Models.Item() { Id = 1 });
        }
     
        [TestMethod]
        public async Task DeleteItem()
        {
            Assert.AreEqual(true, await ViewModel.DeleteItemAsync());
        }

        [TestMethod]
        public async Task UpdateItem()
        {
            ViewModel.Model.Title = "Title change from API at " + DateTime.Now;
            ViewModel.Model.IsArchived = true;
            ViewModel.Model.IsStarred = false;
            Assert.AreEqual(true, await ViewModel.UpdateItemAsync());
            Assert.AreEqual(true, ViewModel.Model.IsArchived);
            Assert.AreEqual(false, ViewModel.Model.IsStarred);
        }

        [TestMethod]
        public async Task FetchItem()
        {
            Assert.AreEqual(true, await ViewModel.FetchInformationForItemAsync());
        }

        //[TestMethod]
        //public async Task GetTags()
        //{
        //}

        //[TestMethod]
        //public async Task AddTags()
        //{
        //}

        //[TestMethod]
        //public async Task DeleteOneTag()
        //{
        //}
    }
}
