using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.Collections.Generic;

namespace wallabag.DataModel
{
    [ImplementPropertyChanged]
    public class User
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("salt")]
        public string Salt { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("is_active")]
        public bool IsActive { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("entries")]
        // TODO: Find a better way, e.g. a ObservableCollection, List, Dictionary etc.
        public object Entries { get; set; }

        public override string ToString()
        {
            return Username;
        }
    }
}
