using System.Text.Json.Serialization;

namespace Theory.Providers.BandCamp.Entities
{
    internal struct BandCampCurrent
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("streaming")]
        public long Streaming { get; set; }

        [JsonPropertyName("id")]
        public long Id { get; set; }
    }
}