using SQLite;

namespace wallabag.Models
{
    [Table("Actions")]
    class OfflineAction
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int ItemId { get; set; }
        public string TagsString { get; set; }
        public int TagId { get; set; }
        public string Url { get; set; }

        public OfflineActionTask Task { get; set; }
        
        public enum OfflineActionTask
        {
            AddItem,
            DeleteItem,
            AddTags,
            DeleteTag,   
            SwitchReadStatus,
            SwitchFavoriteStatus
        }
    }
}
