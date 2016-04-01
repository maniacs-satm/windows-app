using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PropertyChanged;
using SQLite;

namespace wallabag.Models
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
        public string Title { get; set; } = string.Empty;

        [JsonProperty("url")]
        public string Url { get; set; } = string.Empty;

        [JsonProperty("is_archived")]
        public bool IsRead { get; set; } = false;

        [JsonProperty("is_starred")]
        public bool IsStarred { get; set; } = false;
        
        [JsonProperty("content")]
        public string Content { get; set; } = string.Empty;

        [JsonProperty("created_at")]
        public DateTime CreationDate { get; set; } = DateTime.Now;

        [JsonProperty("updated_at")]
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        [JsonProperty("reading_time")]
        public int EstimatedReadingTime { get; set; } = 0;

        [JsonProperty("domain_name")]
        public string DomainName { get; set; } = string.Empty;

        [JsonProperty("mimetype")]
        public string Mimetype { get; set; } = "text/html";

        [JsonProperty("lang")]
        public string Language { get; set; } = string.Empty;

        [JsonProperty("tags")]
        public ICollection<Tag> Tags { get; set; } = new ObservableCollection<Tag>();

        [JsonProperty("preview_picture")]
        public string PreviewPictureUri { get; set; } = string.Empty;

        public string ReadingProgress { get; set; } = string.Empty;

        public override string ToString() { return Title; }

        public Task<int> UpdateAsync()
        {
            this.LastUpdated = DateTime.Now;
            return new SQLiteAsyncConnection(Common.Helpers.DATABASE_PATH).UpdateAsync(this);
        }

        public override bool Equals(object obj)
        {
            var comparedItem = obj as Item;
            return Id.Equals(comparedItem.Id) && CreationDate.Equals(comparedItem.CreationDate);
        }
        public override int GetHashCode() => Id;
    }
}
