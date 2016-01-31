using PropertyChanged;
using System;
using wallabag.Models;

namespace wallabag.Data.Models
{
    [ImplementPropertyChanged]
    public class FilterProperties
    {
        public FilterPropertiesItemType ItemType { get; set; } = FilterPropertiesItemType.Unread;
        public FilterPropertiesSortOrder SortOrder { get; set; } = FilterPropertiesSortOrder.Descending;
        public Tag FilterTag { get; set; }
        public string DomainName { get; set; }
        public int? MinimumEstimatedReadingTime { get; set; }
        public int? MaximumEstimatedReadingTime { get; set; }
        public DateTimeOffset? CreationDateFrom { get; set; }
        public DateTimeOffset? CreationDateTo { get; set; }

        public enum FilterPropertiesSortOrder { Ascending, Descending }
        public enum FilterPropertiesItemType { All, Unread, Favorites, Archived, Deleted }
    }
}
