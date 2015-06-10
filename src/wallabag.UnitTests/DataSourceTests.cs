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
        [DataRow("http://www.sueddeutsche.de/politik/g-gipfel-politik-nach-schlossherrenart-1.2508802", "politik")]
        [DataRow("http://www.sueddeutsche.de/politik/griechenland-nichts-gelernt-1.2507116", "griechenland,politik")]
        [DataRow("http://www.sueddeutsche.de/politik/wagenknecht-auf-linken-parteitag-in-bielefeld-gegen-die-luegner-aus-der-trueben-bruehe-1.2508967", "politik")]
        public async Task AddItem(string Url, string Tags)
        {
            Assert.AreEqual(true, await DataSource.AddItem(Url, Tags));
        }
    }
}
