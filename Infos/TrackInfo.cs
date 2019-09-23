using Theory.Providers;

namespace Theory.Infos
{
    public struct TrackInfo
    {
        public string Id { get; private set; }
        public string Title { get; private set; }
        public string Url { get; private set; }
        public long Duration { get; private set; }
        public string ArtworkUrl { get; private set; }
        public bool CanStream { get; private set; }
        public string Provider { get; private set; }
        public AuthorInfo Author { get; private set; }

        public TrackInfo WithId(string id)
        {
            Id = id;
            return this;
        }

        public TrackInfo WithTitle(string title)
        {
            Title = title;
            return this;
        }

        public TrackInfo WithUrl(string url)
        {
            Url = url;
            return this;
        }

        public TrackInfo WithDuration(long duration)
        {
            Duration = duration;
            return this;
        }

        public TrackInfo WithArtwork(string artwork)
        {
            ArtworkUrl = artwork;
            return this;
        }

        public TrackInfo WithCanStream(bool canStream)
        {
            CanStream = canStream;
            return this;
        }

        public TrackInfo WithProvider(ProviderType provider)
        {
            Provider = $"{provider}";
            return this;
        }

        public TrackInfo WithAuthor(AuthorInfo authorInfo)
        {
            Author = authorInfo;
            return this;
        }
    }
}