using System.Text.Json.Serialization;
using Theory.Infos;

namespace Theory.Providers.YouTube.Entities
{
    public sealed class YouTubeVideo : YouTubeResult
    {
        [JsonPropertyName("length_seconds")]
        public long Duration { get; set; }

        [JsonPropertyName("user_id")]
        public string UserId { get; set; } // Figure out what ID this is

        [JsonIgnore]
        public TrackInfo AsTrackInfo
            => new TrackInfo()
                .WithId(Id)
                .WithAuthor(new AuthorInfo().WithName(Author))
                .WithTitle(Title)
                .WithDuration(Duration * 1000)
                .WithProvider(ProviderType.YouTube)
                .WithUrl($"https://www.youtube.com/watch?v={Id}")
                .WithArtwork($"https://img.youtube.com/vi/{Id}/maxresdefault.jpg");
    }
}