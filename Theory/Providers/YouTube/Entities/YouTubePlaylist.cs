using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Theory.Infos;

namespace Theory.Providers.YouTube.Entities
{
    public sealed class YouTubePlaylist : YouTubeResult
    {
        [JsonPropertyName("video")]
        public IEnumerable<YouTubeVideo> Videos { get; set; }

        public PlaylistInfo BuildPlaylistInfo(string id, string url)
            => new PlaylistInfo()
                .WithId(id)
                .WithUrl(url)
                .WithName(Title)
                .WithDuration(Videos.Sum(x => x.Duration * 1000));
    }
}