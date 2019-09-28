using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Theory.Providers.YouTube.Entities;
using Theory.Providers.YouTube.Entities.YouTubeCipher;

namespace Theory.Providers.YouTube
{
    public readonly struct YouTubeTrackLoader
    {
        private const string BASE_URL = "https://www.youtube.com";
        private const string BASE_GOOGLE_API_URL = "https://youtube.googleapis.com/v";

        private static readonly RestClient _restClient = new RestClient(default);

        private static readonly Dictionary<string, IReadOnlyCollection<IYouTubeCipherOp>> _youTubeCipherOpsCache
            = new Dictionary<string, IReadOnlyCollection<IYouTubeCipherOp>>();

        private static readonly Regex PLAYER_CONFIG_REGEX
            = new Regex(
                "ytplayer\\.config = (?<Json>\\{[^\\{\\}]*(((?<Open>\\{)[^\\{\\}]*)+((?<Close-Open>\\})[^\\{\\}]*)+)*(?(Open)(?!))\\})"
                , RegexOptions.Compiled);

        private static readonly Regex PLAYER_EMBED_CONFIG_REGEX
            = new Regex(
                "yt\\.setConfig\\({'PLAYER_CONFIG':(.*)}\\);"
                , RegexOptions.Compiled);

        private static readonly Regex VIDEO_UNAVALAIBLE_REGEX
            = new Regex("id=\"unavailable-message\" class=\"message\">(.*)</", RegexOptions.Singleline);

        private static readonly Regex DECIPHER_FUNC_NAME_REGEX
            = new Regex("(\\w+)=function\\(\\w+\\){(\\w+)=\\2\\.split\\(\x22{2}\\);.*?return\\s+\\2\\.join\\(\x22{2}\\)}"
                , RegexOptions.Compiled);

        private static readonly Regex DECIPHER_DEFINITION_NAME_REGEX
            = new Regex("(\\w+).\\w+\\(\\w+,\\d+\\);", RegexOptions.Compiled);

        private static readonly Regex DECIPHER_CALLED_FUNC_NAME_REGEX
            = new Regex(@"\w+(?:.|\[)(\""?\w+(?:\"")?)\]?\(", RegexOptions.Compiled);

        private static readonly Regex SLICE_SWAP_REGEX
            = new Regex("\\(\\w+,(\\d+)\\)", RegexOptions.Compiled);

        private static readonly Regex SIGNATURE_REGEX
            = new Regex("/s/(.*?)(?:/|$)", RegexOptions.Compiled);

        private static readonly Regex LENGTH_REGEX
            = new Regex("clen[/=](\\d+)", RegexOptions.Compiled);

        private static readonly Regex AUDIO_TYPE_REGEX
            = new Regex("mime[/=]\\w*%2F([\\w\\d]*)", RegexOptions.Compiled);

        private static readonly Regex RATE_LIMIT_REGEX
            = new Regex("ratebypass[=/]yes", RegexOptions.Compiled);

        public static async ValueTask<Stream> LoadTrackAsync(string trackId)
        {
            var audioStreamsInfo = await GetAudioStreamsInfoAsync(trackId).ConfigureAwait(false);

            var bestAudioStreamInfo = audioStreamsInfo
                .OrderByDescending(a => a.Bitrate)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(bestAudioStreamInfo.Url))
                throw new Exception($"Can't get a available audio for this track: {trackId}.");

            var stream = await _restClient
                 .WithUrl(bestAudioStreamInfo.Url)
                 .WithRange(bestAudioStreamInfo.Length)
                 .GetStreamAsync()
                 .ConfigureAwait(false);

            return stream;
        }

        private static async ValueTask<ICollection<YouTubeAudioStreamInfo>>
            GetAudioStreamsInfoAsync(string trackId)
        {
            var playerconfig = await GetPlayerConfigAsync(trackId).ConfigureAwait(false);

            var streamInfoDics = playerconfig.StreamsInfoUrlEncoded
                .Split(',')
                .Select(SplitInfo);

            var audiosInfo = new List<YouTubeAudioStreamInfo>();

            #region Audios Foreach

            foreach (var streamInfoDic in streamInfoDics)
            {
                if (streamInfoDic["type"].StartsWith("audio/", StringComparison.OrdinalIgnoreCase))
                {
                    var id = int.Parse(streamInfoDic["itag"]);
                    var url = streamInfoDic["url"];
                    var bitrate = long.Parse(streamInfoDic["bitrate"]);

                    var signature = streamInfoDic.GetValueOrDefault("s");
                    if (!string.IsNullOrWhiteSpace(signature))
                    {
                        var ciphersOps = await GetCipherOpsAsync(playerconfig.PlayerSourceUrl)
                            .ConfigureAwait(false);

                        signature = Decipher(ciphersOps, signature);

                        var signatureParam = streamInfoDic.GetValueOrDefault("sp") ?? "signature";
                        url = url.WithParameter(signatureParam, signature);
                    }

                    long? length = long.Parse(streamInfoDic.GetValueOrDefault("clen"));
                    if (length <= 0)
                    {
                        length = await _restClient.GetContentLengthAsync(url).ConfigureAwait(false);

                        if (!length.HasValue || length <= 0)
                            continue;
                    }

                    var type = GetAudioType(streamInfoDic["type"]
                        .SubstringUntil(";")
                        .SubstringAfter("/"));

                    var encoding = GetAudioEncoding(
                        streamInfoDic["type"]
                        .SubstringAfter("codecs=\"")
                        .SubstringUntil("\""));

                    var rateLimited = !RATE_LIMIT_REGEX.IsMatch(url);

                    audiosInfo.Add(
                        new YouTubeAudioStreamInfo(
                            id, bitrate, url, length.Value, rateLimited, type, encoding)
                        );
                }
            }

            #endregion Audios Foreach

            #region DashManifest

            // Thanks to YouTube Explode to DashManifestXml reading.
            // https://github.com/Tyrrrz/YoutubeExplode

            var dashManifestUrl = playerconfig.DashManifestUrl;
            if (!string.IsNullOrWhiteSpace(dashManifestUrl))
            {
                var signatureMatch = SIGNATURE_REGEX.Match(dashManifestUrl);
                var signature = signatureMatch.Groups[1].Value;

                if (!string.IsNullOrWhiteSpace(signature))
                {
                    var cipherOps = await GetCipherOpsAsync(playerconfig.PlayerSourceUrl)
                        .ConfigureAwait(false);

                    signature = Decipher(cipherOps, signature);

                    dashManifestUrl = dashManifestUrl
                        .WithParameter("signature", signature);
                }

                var dashManifestXml = await GetDashManifestXmlAsync(dashManifestUrl)
                    .ConfigureAwait(false);

                var streamInfosXmls = dashManifestXml
                    .Descendants()
                    .Where(
                        a => a
                              .Descendants("Initialization")
                              .FirstOrDefault()?
                              .Attribute("sourceURL")?
                              .Value
                              .Contains("sq/") != true
                    );

                foreach (var streamInfoXml in streamInfosXmls)
                {
                    if (streamInfoXml.Element("AudioChannelConfiguration") != null)
                    {
                        var id = int.Parse(streamInfoXml.Attribute("id").Value);
                        var url = streamInfoXml.Element("BaseURL").Value;

                        var lengthMatch = LENGTH_REGEX.Match(url);
                        var length = long.Parse(lengthMatch.Groups[1].Value);

                        var bitrate = long.Parse(streamInfoXml.Attribute("bandwidth").Value);

                        var typeMatch = AUDIO_TYPE_REGEX.Match(url);
                        var typeString = WebUtility.UrlDecode(typeMatch.Groups[1].Value);
                        var type = GetAudioType(typeString);

                        var audioEncodingString = streamInfoXml.Attribute("codecs").Value;
                        var encoding = GetAudioEncoding(audioEncodingString);

                        var rateLimited = !RATE_LIMIT_REGEX.IsMatch(url);

                        audiosInfo.Add(
                            new YouTubeAudioStreamInfo(
                                id, bitrate, url, length, rateLimited, type, encoding)
                            );
                    }
                }
            }

            #endregion DashManifest

            return audiosInfo;
        }

        private static async ValueTask<YouTubePlayerConfig> GetPlayerConfigAsync(string trackId)
        {
            var videoEmbedPage = await GetVideoEmbedPageAsync(trackId).ConfigureAwait(false);
            var playerConfigMatch = PLAYER_EMBED_CONFIG_REGEX.Match(videoEmbedPage);
            var playerConfigJson = JsonDocument.Parse(playerConfigMatch.Groups[1].Value);

            var playerSourceUrl = GetPlayerSourceUrl(playerConfigJson.RootElement);
            var dic = await GetVideoInfoAsync(trackId).ConfigureAwait(false);
            var playerResponseJson = JsonDocument.Parse(dic["player_response"]);

            var root = playerResponseJson.RootElement;
            var playabilityStatus = root.GetToken("playabilityStatus.status");

            // if error throw exception
            if ("error".Equals(playabilityStatus, StringComparison.OrdinalIgnoreCase))
                throw new Exception($"The track {trackId} is unavailabe.");

            // verifying purchase...

            if (root.TryGetToken("playabilityStatus.errorScreen.playerLegacyDesktopYpcTrailerRenderer" +
                ".trailerVideoId", out var _))
                throw new Exception($"Can't load track {trackId} because it needs to purchase.");

            if (root.TryGetToken("playabilityStatus.errorScreen.ypcTrailerRenderer.playerVars", out var _))
                throw new Exception($"Can't load track {trackId} because it needs to purchase.");

            root.TryGetToken("streamingData.dashManifestUrl", out var dashManifestUrl);
            var streamsInfoUrl = dic.GetValueOrDefault("adaptive_fmts");

            // if don't have a error we can return this.
            if (!root.TryGetToken("playabilityStatus.reason", out var _))
                return new YouTubePlayerConfig(playerSourceUrl, streamsInfoUrl, dashManifestUrl);

            // if can't, we try from watch page

            var page = await GetVideoWatchPageAsync(trackId).ConfigureAwait(false);

            var match = PLAYER_CONFIG_REGEX.Match(page);

            var playerconfig = match.Groups["Json"].Value;

            // if playerconfig is null or empty throw an exception
            if (string.IsNullOrWhiteSpace(playerconfig))
            {
                match = VIDEO_UNAVALAIBLE_REGEX.Match(page);

                var reason = match.Groups[1].Value
                    .Replace("\n", "")
                    .Trim();

                throw new Exception($"Can't load track {trackId}, reason: {reason}.");
            }

            playerResponseJson = JsonDocument.Parse(playerconfig);
            root = playerResponseJson.RootElement;

            playerSourceUrl = GetPlayerSourceUrl(root);

            playerResponseJson = JsonDocument.Parse(root.GetToken("args.player_response"));
            root = playerResponseJson.RootElement;

            root.TryGetToken("streamingData.dashManifestUrl", out dashManifestUrl);
            root.TryGetToken("args.adaptive_fmts", out streamsInfoUrl);

            return new YouTubePlayerConfig(playerSourceUrl, streamsInfoUrl, dashManifestUrl);
        }

        private static async ValueTask<IReadOnlyCollection<IYouTubeCipherOp>>
            GetCipherOpsAsync(string playerSourceUrl)
        {
            //Ciphers operations don't change in the same playerSourceUrl, with it whe can cache they.

            if (_youTubeCipherOpsCache.TryGetValue(playerSourceUrl, out var cacheCiphers))
                return cacheCiphers;

            var playerSource = await _restClient
                .WithUrl(playerSourceUrl)
                .WithHeader("User-Agent",
                                "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0")
                .GetStringAsync()
                .ConfigureAwait(false);

            var decipherFuncNameMatch = DECIPHER_FUNC_NAME_REGEX.Match(playerSource);
            var decipherFuncName = decipherFuncNameMatch.Groups[1].Value;

            if (string.IsNullOrWhiteSpace(decipherFuncName))
                throw new Exception($"Can'get signature decipher function name from {playerSourceUrl}.");

            var decipherFuncBodyMatch =
                Regex.Match(
                    playerSource,
                    $"(?!h\\.){Regex.Escape(decipherFuncName)}{@"=function\(\w+\)\{(.*?)\}"}",
                    RegexOptions.Singleline);
            var decipherFuncBody = decipherFuncBodyMatch.Groups[1].Value;

            if (string.IsNullOrWhiteSpace(decipherFuncName))
                throw new Exception($"Can'get signature decipher function body from {playerSourceUrl}.");

            var decipherFuncBodyStatements = decipherFuncBody.Split(";");

            var decipherDefinitionNameMatch = DECIPHER_DEFINITION_NAME_REGEX.Match(decipherFuncBody);
            var decipherDefinitionName = decipherDefinitionNameMatch.Groups[1].Value;

            var decipherDefinitionBodyMatch =
                Regex.Match(playerSource,
                "var\\s+" +
                Regex.Escape(decipherDefinitionName) +
                @"=\{(\w+:function\(\w+(,\w+)?\)\{(.*?)\}),?\};", RegexOptions.Singleline);
            var decipherDefinitionBody = decipherDefinitionBodyMatch.Groups[0].Value;

            var deciphersOps = new List<IYouTubeCipherOp>();

            foreach (var statement in decipherFuncBodyStatements)
            {
                var calledFuncNameMatch = DECIPHER_CALLED_FUNC_NAME_REGEX.Match(statement);
                var calledFuncName = calledFuncNameMatch.Groups[1].Value;

                if (string.IsNullOrWhiteSpace(calledFuncName))
                    continue;

                var isSlice = Regex.IsMatch(
                    decipherDefinitionBody,
                    $@"{Regex.Escape(calledFuncName)}:\bfunction\b\([a],b\).(\breturn\b)?.?\w+\.",
                    RegexOptions.Compiled);

                var isSwap = Regex.IsMatch(
                    decipherDefinitionBody,
                    $@"{Regex.Escape(calledFuncName)}:\bfunction\b\(\w+\,\w\).\bvar\b.\bc=a\b",
                    RegexOptions.Compiled);

                var isReverse = Regex.IsMatch(
                    decipherDefinitionBody,
                    $@"{Regex.Escape(calledFuncName)}:\bfunction\b\(\w+\)",
                    RegexOptions.Compiled);

                if (isSlice)
                {
                    var indexMatch = SLICE_SWAP_REGEX.Match(statement);
                    var index = int.Parse(indexMatch.Groups[1].Value);
                    deciphersOps.Add(new YouTubeSliceCipherOp(index));
                }
                else if (isSwap)
                {
                    var indexMatch = SLICE_SWAP_REGEX.Match(statement);
                    var index = int.Parse(indexMatch.Groups[1].Value);
                    deciphersOps.Add(new YouTubeSwapCipherOp(index));
                }
                else if (isReverse)
                {
                    deciphersOps.Add(new YouTubeReverseCipherOp(default));
                }
            }

            _youTubeCipherOpsCache[playerSourceUrl] = deciphersOps;
            return deciphersOps;
        }

        private static async ValueTask<string> GetVideoWatchPageAsync(string trackId)
        {
            var page = await _restClient
                .WithUrl(BASE_URL)
                .WithPath("watch")
                .WithParameter("v", trackId)
                .WithParameter("disable_polymer", "true")
                .WithParameter("bpctr", "9999999999")
                .WithParameter("hl", "en")
                .WithHeader("User-Agent",
                                "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0")
                .GetStringAsync()
                .ConfigureAwait(false);

            return page;
        }

        private static async ValueTask<string> GetVideoEmbedPageAsync(string trackId)
        {
            var page = await _restClient
                .WithUrl(BASE_URL)
                .WithPath("embed")
                .WithPath(trackId)
                .WithParameter("disable_polymer", "true")
                .WithParameter("hl", "en")
                .WithHeader("User-Agent",
                                "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0")
                .GetStringAsync()
                .ConfigureAwait(false);

            return page;
        }

        private static async ValueTask<XElement> GetDashManifestXmlAsync(string url)
        {
            var page = await _restClient
                .WithUrl(url)
                .WithHeader("User-Agent",
                                "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0")
                .GetStringAsync()
                .ConfigureAwait(false);

            var xml = XElement.Parse(page).StripNamespaces();

            return xml;
        }

        private static string Decipher(IEnumerable<IYouTubeCipherOp> ops, string signature)
        {
            foreach (var op in ops)
                signature = op.Decipher(signature);

            return signature;
        }

        private static string GetPlayerSourceUrl(JsonElement root)
            => $"{BASE_URL}{root.GetToken("assets.js")}";

        private static Dictionary<string, string> SplitInfo(string input)
        {
            var paramsEncoded = input.TrimStart('?').Split("&");

            var dic = new Dictionary<string, string>();
            foreach (var paramEncoded in paramsEncoded)
            {
                var param = WebUtility.UrlDecode(paramEncoded);

                var equalsPos = param.IndexOf('=');
                if (equalsPos <= 0)
                    continue;

                var key = param.Substring(0, equalsPos);
                var value = equalsPos < param.Length
                    ? param.Substring(equalsPos + 1)
                    : string.Empty;

                dic[key] = value;
            }

            return dic;
        }

        private static async ValueTask<Dictionary<string, string>> GetVideoInfoAsync(string trackId)
        {
            var vidInfo = await _restClient
                            .WithUrl(BASE_URL)
                            .WithPath("get_video_info")
                            .WithParameter("video_id", trackId)
                            .WithParameter("el", "embedded")
                            .WithParameter("eurl", $"{BASE_GOOGLE_API_URL}{trackId}")
                            .WithParameter("hl", "en")
                            .WithHeader("User-Agent",
                                "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0")
                            .GetBytesAsync()
                            .ConfigureAwait(false);

            if (vidInfo.IsEmpty)
                throw new Exception("Provider didn't return any stream information.'");

            var vidInfoString = Encoding.UTF8.GetString(vidInfo.ToArray(), 0, vidInfo.Length);

            return SplitInfo(vidInfoString);
        }

        private static YouTubeAudioEncoding GetAudioEncoding(string rawAudio)
        {
            return rawAudio switch
            {
                _ when rawAudio.StartsWith("mp4a", StringComparison.OrdinalIgnoreCase)
                    => YouTubeAudioEncoding.Aac,
                _ when rawAudio.StartsWith("vorbis", StringComparison.OrdinalIgnoreCase)
                    => YouTubeAudioEncoding.Vorbis,
                _ when rawAudio.StartsWith("opus", StringComparison.OrdinalIgnoreCase)
                    => YouTubeAudioEncoding.Opus,
                _ => throw new ArgumentException($"Can't provide the {nameof(YouTubeAudioEncoding)} " +
                $"for {rawAudio}", nameof(rawAudio))
            };
        }

        private static YouTubeAudioType GetAudioType(string raw)
        {
            return raw switch
            {
                _ when "mp4".Equals(raw, StringComparison.OrdinalIgnoreCase) => YouTubeAudioType.Mp4,
                _ when "webm".Equals(raw, StringComparison.OrdinalIgnoreCase) => YouTubeAudioType.WebM,
                _ when "3gpp".Equals(raw, StringComparison.OrdinalIgnoreCase) => YouTubeAudioType.Trdgpp,
                _ => throw new ArgumentException($"Can't provide the {nameof(YouTubeAudioType)} " +
                $"for {raw}", nameof(raw))
            };
        }
    }
}