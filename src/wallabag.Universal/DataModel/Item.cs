using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PropertyChanged;
using SQLite;

namespace wallabag.DataModel
{
    #region required for parsing
    public class RootObject
    {
        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("pages")]
        public int Pages { get; set; }

        [JsonProperty("limit")]
        public int Limit { get; set; }

        [JsonProperty("total")]
        public int TotalNumberOfItems { get; set; }

        [JsonProperty("_embedded")]
        public _Embedded Embedded { get; set; }
    }
    public class _Embedded
    {
        [JsonProperty("items")]
        public Item[] Items { get; set; }
    }
    #endregion

    [ImplementPropertyChanged]
    [Table("Items")]
    public class Item
    {
        [PrimaryKey]
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("is_archived")]
        public bool IsArchived { get; set; }

        [JsonProperty("is_starred")]
        public bool IsStarred { get; set; }

        [JsonProperty("is_deleted")]
        public bool IsDeleted { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [Ignore]
        [JsonProperty("tags")]
        public List<Tag> Tags { get; set; }

        public string TagsString { get; set; }

        public float ReadingProgress { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
