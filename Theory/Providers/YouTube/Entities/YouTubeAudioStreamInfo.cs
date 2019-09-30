namespace Theory.Providers.YouTube.Entities
{
    internal readonly struct YouTubeAudioStreamInfo
    {
        public readonly YouTubeAudioEncoding Encoding;

        public readonly YouTubeAudioType Type;

        public readonly int Id;

        public readonly long Bitrate;

        public readonly string Url;

        public readonly long Length;

        public readonly bool IsRateLimited;

        internal YouTubeAudioStreamInfo(
            int id,
            long bitrate,
            string url,
            long length,
            bool rateLimited,
            YouTubeAudioType type,
            YouTubeAudioEncoding encoding)
        {
            Encoding = encoding;
            Type = type;
            Id = id;
            Bitrate = bitrate;
            Url = url;
            Length = length;
            IsRateLimited = rateLimited;
        }
    }
}