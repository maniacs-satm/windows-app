using PropertyChanged;

namespace wallabag.Models
{
    [ImplementPropertyChanged]
    public class SearchResult
    {
        public int Id { get; set; } = 0;
        public string Value { get; set; } = string.Empty;

        public SearchResult(int id, string value)
        {
            Id = id;
            Value = value;
        }

        public override string ToString() { return Value; }
    }
}
