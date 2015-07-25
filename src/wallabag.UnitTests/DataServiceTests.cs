using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using wallabag.Models;
using wallabag.Services;

namespace wallabag.UnitTests
{
    [TestClass]
    public class DataServiceTests
    {
        [TestMethod]
        public async Task GetItems()
        {
            var collection = await DataService.GetItemsAsync(new FilterProperties());
            CollectionAssert.AllItemsAreUnique(collection);
            CollectionAssert.AllItemsAreInstancesOfType(collection, typeof(Item));
        }

        [TestMethod]
        public async Task GetSingleItem()
        {
            Item i = await DataService.GetItemAsync(1);
            Assert.AreEqual(1, i.Id);
        }

        [TestMethod]
        [DataRow("http://www.sueddeutsche.de/politik/g-gipfel-politik-nach-schlossherrenart-1.2508802", "politik")]
        [DataRow("http://www.sueddeutsche.de/politik/griechenland-nichts-gelernt-1.2507116", "griechenland,politik")]
        [DataRow("http://www.sueddeutsche.de/politik/wagenknecht-auf-linken-parteitag-in-bielefeld-gegen-die-luegner-aus-der-trueben-bruehe-1.2508967", "politik")]
        public async Task AddItem(string Url, string Tags)
        {
            Assert.AreEqual(true, await DataService.AddItemAsync(Url, Tags));
        }
    }
}
