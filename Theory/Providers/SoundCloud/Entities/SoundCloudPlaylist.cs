using System.Collections.Generic;
using System.Text.Json.Serialization;
using Theory.Infos;

namespace Theory.Providers.SoundCloud.Entities
{
    public sealed class SoundCloudPlaylist : SoundCloudResult
    {
        [JsonPropertyName("tracks")]
        public IList<SoundCloudTrack> Tracks { get; set; }

        [JsonIgnore]
        public PlaylistInfo AsPlaylistInfo
            => new PlaylistInfo()
                .WithId($"{Id}")
                .WithName(Title)
                .WithUrl(PermalinkUrl)
                .WithDuration(Duration)
                .WithArtwork(ArtworkUrl);
    }
}