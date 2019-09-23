using System;
using System.Text.RegularExpressions;

namespace Theory.Providers.YouTube
{
    public readonly struct YouTubeParser
    {
        public static ReadOnlySpan<byte> Type
            => Extensions.ToBytes("type");

        public static ReadOnlySpan<byte> Bitrate
            => Extensions.ToBytes("bitrate");

        public static ReadOnlySpan<byte> Url
            => Extensions.ToBytes("url");

        public static Regex IdRegex
            => new Regex("(?!videoseries)[a-zA-Z0-9_-]{11,42}",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static void ParseId(string url, out string videoId, out string playlistId)
        {
            var matches = IdRegex.Matches(url);
            var (vidId, plyId) = ("", "");

            foreach (Match match in matches)
            {
                if (!match.Success)
                    continue;

                if (match.Length == 11)
                    vidId = match.Value;
                else
                    plyId = match.Value;
            }

            videoId = vidId;
            playlistId = plyId;
        }

        public static bool ToKeyValuePair(ref ReadOnlySpan<byte> span, out ReadOnlySpan<byte> key,
            out ReadOnlySpan<byte> value)
        {
            key = Span<byte>.Empty;
            value = Span<byte>.Empty;

            if (span == default || span.Length == 0)
                return false;

            var keyIndex = span.IndexOf((byte) '=');
            if (keyIndex < 0)
                return false;

            key = span.Slice(0, keyIndex);
            span = span.Slice(keyIndex);

            if (span[0] != '=')
                return false;

            span = span.Slice(1);

            var valueIndex = span.IndexOf((byte) '&');
            if (valueIndex < 0)
            {
                value = span;
                span = Span<byte>.Empty;

                return true;
            }

            value = span.Slice(0, valueIndex);
            span = span.Slice(valueIndex + 1);
            return true;
        }
    }
}