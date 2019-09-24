using System.Text.Json.Serialization;

namespace Theory.Providers.YouTube.Entities
{
    public class YouTubeResult
    {
        [JsonPropertyName("encrypted_id")]
        public string Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("author")]
        public string Author { get; set; }
    }
}