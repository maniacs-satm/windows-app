using Newtonsoft.Json;
using SQLite;

namespace wallabag.Models
{
    [Table("Tags")]
    public class Tag
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        public override string ToString() { return Label; }

        public Tag()
        {
            Id = -1;
            Label = string.Empty;
        }
    }
}
