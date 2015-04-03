using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using wallabag.DataModel;
using System.Threading.Tasks;
using Windows.Storage;

namespace wallabag.Tests
{
    [TestClass]
    public class DataSourceTests
    {
        [TestMethod]
        public async Task GetItems()
        {
            Assert.AreEqual(true, await DataSource.GetItemsAsync());
            CollectionAssert.AllItemsAreUnique(DataSource.Items);
        }

        [TestMethod]
        public async Task GetSingleItem()
        {
            Assert.AreEqual(true, await DataSource.GetItemsAsync());
            CollectionAssert.AllItemsAreUnique(DataSource.Items);
        }

        [TestMethod]
        public async Task GetSaveAndRestoreItems()
        {
            await GetItems();
            await SaveItems();
            await RestoreItems();
            await RestoreItemsWithoutFile();
        }

        [TestMethod]
        public async Task SaveItems()
        {
            Assert.AreEqual(true, await DataSource.SaveItemsAsync());
        }

        public async Task RestoreItems()
        {
            Assert.AreEqual(true, await DataSource.RestoreItemsAsync());
            Assert.IsNotNull(DataSource.Items);
            CollectionAssert.AllItemsAreUnique(DataSource.Items);
        }

        public async Task RestoreItemsWithoutFile()
        {
            StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("items.json");
            await file.DeleteAsync();
            Assert.AreEqual(false, await DataSource.RestoreItemsAsync());
        }

        [TestMethod]
        [Ignore]
        public async Task AddItem()
        {
            await DataSource.AddItem("http://www.neowin.net/news/grand-theft-auto-v-for-the-pc-shown-running-at-60fps", "gaming");
        }
    }
}
