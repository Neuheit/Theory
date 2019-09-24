using System.Text.Json.Serialization;

namespace Theory.Providers.BandCamp.Entities
{
    internal struct BandCampFile
    {
        [JsonPropertyName("mp3-128")]
        public string Mp3Url { get; set; }
    }
}