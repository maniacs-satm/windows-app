using wallabag.Models;

namespace wallabag.Data.Models
{
    public class DownloadProgress
    {
        public int TotalNumberOfItems { get; set; }
        public int CurrentItemIndex { get; set; }
        public Item CurrentItem { get; set; }
    }
}
