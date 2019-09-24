using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Theory.Providers.YouTube.Entities
{
    public struct YouTubeSearch
    {
        [JsonPropertyName("video")]
        public IEnumerable<YouTubeVideo> Video { get; set; }
    }
}