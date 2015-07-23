using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public OfflineActionTask Task { get; set; }
        
        public enum OfflineActionTask
        {
            AddItem,
            DeleteItem,
            ModifyTags,
            SwitchReadStatus,
            SwitchFavoriteStatus
        }
    }
}
