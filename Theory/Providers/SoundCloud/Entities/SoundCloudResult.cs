using System.Text.Json.Serialization;

namespace Theory.Providers.SoundCloud.Entities
{
    public class SoundCloudResult
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("duration")]
        public long Duration { get; set; }

        [JsonPropertyName("streamable")]
        public bool IsStreamable { get; set; }

        [JsonPropertyName("artwork_url")]
        public string ArtworkUrl { get; set; }

        [JsonPropertyName("permalink_url")]
        public string PermalinkUrl { get; set; }

        [JsonPropertyName("user")]
        public SoundCloudUser User { get; set; }
    }
}