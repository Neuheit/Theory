using System.Text.Json.Serialization;
using Theory.Infos;

namespace Theory.Providers.SoundCloud.Entities
{
    public sealed class SoundCloudTrack : SoundCloudResult
    {
        [JsonIgnore]
        public TrackInfo AsTrackInfo
            => new TrackInfo()
                .WithId($"{Id}")
                .WithTitle(Title)
                .WithUrl(PermalinkUrl)
                .WithDuration(Duration)
                .WithArtwork(ArtworkUrl)
                .WithCanStream(IsStreamable)
                .WithProvider(ProviderType.SoundCloud)
                .WithAuthor(User.AsAuthorInfo);
    }
}