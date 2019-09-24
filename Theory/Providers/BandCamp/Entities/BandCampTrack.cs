using System;
using System.Text.Json.Serialization;
using Theory.Infos;

namespace Theory.Providers.BandCamp.Entities
{
    internal struct BandCampTrack
    {
        [JsonPropertyName("streaming")]
        public int Streaming { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("track_id")]
        public long TrackId { get; set; }

        [JsonPropertyName("file")]
        public BandCampFile File { get; set; }

        [JsonPropertyName("duration")]
        public double Duration { get; set; }

        public TrackInfo AsTrackInfo(string author, string url, long artId)
            => new TrackInfo()
                .WithId($"{TrackId}")
                .WithTitle(Title)
                .WithUrl(url)
                .WithDuration((long) TimeSpan.FromSeconds(Duration)
                    .TotalMilliseconds)
                .WithArtwork(artId == 0 ? default : $"https://f4.bcbits.com/img/a{artId}_0.jpg")
                .WithCanStream(Streaming == 1)
                .WithProvider(ProviderType.BandCamp)
                .WithAuthor(new AuthorInfo().WithName(author));
    }
}