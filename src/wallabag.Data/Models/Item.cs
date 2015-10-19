﻿using System;
using System.Collections.ObjectModel;
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

        [JsonProperty("is_deleted")]
        public bool IsDeleted { get; set; } = false;

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

        [JsonProperty("tags")]
        public ObservableCollection<Tag> Tags { get; set; } = new ObservableCollection<Tag>();

        [JsonProperty("preview_picture")]
        public Uri PreviewPictureUri { get; set; }

        public string ReadingProgress { get; set; } = "0";

        public override string ToString() { return Title; }
    }
}
