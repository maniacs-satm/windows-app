using SQLite;

namespace wallabag.Models
{
    [Table("OfflineTasks")]
    public class OfflineTask
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int ItemId { get; set; }
        public string TagsString { get; set; }
        public int TagId { get; set; }
        public string Url { get; set; }

        public OfflineTaskType Task { get; set; }

        public enum OfflineTaskType
        {
            AddItem,
            DeleteItem,
            AddTags,
            DeleteTag,
            MarkItemAsRead,
            UnmarkItemAsRead,
            MarkItemAsFavorite,
            UnmarkItemAsFavorite
        }
    }
}
