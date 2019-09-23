using System.Text.Json.Serialization;

namespace Theory.Providers.SoundCloud.Entities
{
    public struct SoundCloudDirectUrl
    {
        [JsonPropertyName("http_mp3_128_url")]
        public string Url { get; set; }
    }
}