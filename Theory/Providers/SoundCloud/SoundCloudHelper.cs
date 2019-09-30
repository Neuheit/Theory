using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Theory.Providers.SoundCloud
{
    public readonly struct SoundCloudHelper
    {
        private static readonly Regex PageScriptRegex
            = new Regex("https://[A-Za-z0-9-.]+/assets/app-[a-f0-9-]+\\.js",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex ScriptClientIdRegex
            = new Regex(",client_id:\"([a-zA-Z0-9-_]+)\"",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static string ClientId { get; private set; }
        private static DateTimeOffset? _lastUpdate;

        public static async Task ValidateClientIdAsync(RestClient restClient)
        {
            if (_lastUpdate.HasValue && _lastUpdate.Value.AddMinutes(50) < DateTimeOffset.Now)
                return;

            var rawPage = await MakeRequestAsync(restClient, "https://soundcloud.com")
                .ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(rawPage))
                throw new NullReferenceException(nameof(rawPage));

            var matchScriptUrl = PageScriptRegex.Match(rawPage)
                .Value;

            var scriptString = await MakeRequestAsync(restClient, matchScriptUrl)
                .ConfigureAwait(false);

            var match = ScriptClientIdRegex.Match(scriptString);
            var id = match.Groups[1]
                .Value;

            ClientId = id;
            _lastUpdate = DateTimeOffset.Now;
        }

        private static async Task<string> MakeRequestAsync(RestClient restClient, string url)
        {
            var rawString = await restClient
                .WithHeader("Accept-Charset", "ISO-8859-1")
                .WithHeader("Accept-Encoding", "gzip, deflate")
                .WithHeader("Accept", "text/html,application/xhtml+xml,application/xml")
                .WithHeader("User-Agent",
                    "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0")
                .GetStringAsync(url)
                .ConfigureAwait(false);

            return rawString;
        }
    }
}