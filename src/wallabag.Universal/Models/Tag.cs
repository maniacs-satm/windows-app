using Newtonsoft.Json;

namespace wallabag.Models
{
    public class Tag
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        public override string ToString()
        {
            return Label;
        }
    }
}
