using System;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Theory.Providers.SoundCloud
{
    public readonly struct SoundCloudHelper
    {
        private static readonly Regex PageScriptRegex
            = new Regex("https://[A-Za-z0-9-.]+/assets/app-[a-f0-9-]+\\.js", RegexOptions.Compiled);

        private static readonly Regex ScriptClientIdRegex
            = new Regex(",client_id:\"([a-zA-Z0-9-_]+)\"", RegexOptions.Compiled);

        public static string ClientId { get; private set; }
        private static DateTimeOffset? _lastUpdate;

        public static async Task ValidateClientIdAsync(RestClient restClient)
        {
            if (_lastUpdate.HasValue && _lastUpdate.Value.AddHours(1) < DateTimeOffset.Now)
                return;

            restClient.WithHeader("Accept-Charset", "ISO-8859-1")
                .WithHeader("Accept-Encoding", "gzip, deflate")
                .WithHeader("Accept", "text/html,application/xhtml+xml,application/xml")
                .WithHeader("User-Agent",
                    "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");

            var stream = await restClient.GetStreamAsync("https://soundcloud.com")
                .ConfigureAwait(false);

            await using var decompressedStream = new GZipStream(stream, CompressionMode.Decompress);
            using var streamReader = new StreamReader(decompressedStream);

            var rawString = await streamReader.ReadToEndAsync()
                .ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(rawString))
                throw new NullReferenceException(nameof(rawString));

            var matchScriptUrl = PageScriptRegex.Match(rawString)
                .Value;

            var match = ScriptClientIdRegex.Match(matchScriptUrl);
            ClientId = match.Groups[1]
                .Value;

            _lastUpdate = DateTimeOffset.Now;
        }
    }
}