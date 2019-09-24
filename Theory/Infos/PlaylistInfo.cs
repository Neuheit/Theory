namespace Theory.Infos
{
    public struct PlaylistInfo
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Url { get; private set; }
        public long Duration { get; private set; }
        public string ArtworkUrl { get; private set; }

        public PlaylistInfo WithId(string id)
        {
            Id = id;
            return this;
        }

        public PlaylistInfo WithName(string name)
        {
            Name = name;
            return this;
        }

        public PlaylistInfo WithUrl(string url)
        {
            Url = url;
            return this;
        }

        public PlaylistInfo WithDuration(long duration)
        {
            Duration = duration;
            return this;
        }

        public PlaylistInfo WithArtwork(string artwork)
        {
            ArtworkUrl = artwork;
            return this;
        }
    }
}