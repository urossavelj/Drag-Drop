namespace API.Models
{
    public class Settings
    {
        public string Id { get; set; }
        public string? Value { get; set; }

        public Settings(string id, string? value)
        {
            Id = id;
            Value = value;
        }
    }
}
