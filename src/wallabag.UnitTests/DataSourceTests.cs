using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using wallabag.DataModel;
using Windows.Storage;

namespace wallabag.UnitTests
{
    [TestClass]
    public class DataSourceTests
    {
        [TestMethod]
        public async Task GetItems()
        {
            Assert.AreEqual(true, await DataSource.GetItemsAsync());
        }

        [TestMethod]
        public async Task GetSingleItem()
        {
            Assert.AreEqual(true, await DataSource.GetItemsAsync());
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
        }

        public async Task RestoreItemsWithoutFile()
        {
            StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("items.json");
            await file.DeleteAsync();
            Assert.AreEqual(false, await DataSource.RestoreItemsAsync());
        }

        [TestMethod]
        [DataRow("http://www.sueddeutsche.de/politik/g-gipfel-politik-nach-schlossherrenart-1.2508802", "politik")]
        [DataRow("http://www.sueddeutsche.de/politik/griechenland-nichts-gelernt-1.2507116", "griechenland,politik")]
        [DataRow("http://www.sueddeutsche.de/politik/wagenknecht-auf-linken-parteitag-in-bielefeld-gegen-die-luegner-aus-der-trueben-bruehe-1.2508967", "politik")]
        public async Task AddItem(string Url, string Tags)
        {
            Assert.AreEqual(true, await DataSource.AddItem(Url, Tags));
        }
    }
}
