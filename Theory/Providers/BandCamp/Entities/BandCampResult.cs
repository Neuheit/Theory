using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Theory.Providers.BandCamp.Entities
{
    internal struct BandCampResult
    {
        [JsonPropertyName("current")]
        public BandCampCurrent Current { get; set; }

        [JsonPropertyName("art_id")]
        public long ArtId { get; set; }

        [JsonPropertyName("trackinfo")]
        public IEnumerable<BandCampTrack> TrackInfo { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("artist")]
        public string Artist { get; set; }

        [JsonPropertyName("item_type")]
        public string ItemType { get; set; }
    }
}