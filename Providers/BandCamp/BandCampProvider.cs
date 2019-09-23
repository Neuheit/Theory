using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Theory.Infos;
using Theory.Interfaces;
using Theory.Providers.BandCamp.Entities;
using Theory.Search;

namespace Theory.Providers.BandCamp
{
    public readonly struct BandCampProvider : IAudioProvider
    {
        private readonly Regex _trackUrlRegex, _albumUrlRegex;

        private readonly RestClient _restClient;

        public BandCampProvider(RestClient restClient)
        {
            _restClient = restClient;
            _trackUrlRegex = new Regex("^https?://(?:[^.]+\\.|)bandcamp\\.com/track/([a-zA-Z0-9-_]+)/?(?:\\?.*|)$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);
            _albumUrlRegex = new Regex("^https?://(?:[^.]+\\.|)bandcamp\\.com/album/([a-zA-Z0-9-_]+)/?(?:\\?.*|)$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        /// <inheritdoc />
        public async ValueTask<SearchResponse> SearchAsync(string query)
        {
            var response = SearchResponse.Create(query);

            query = query switch
            {
                var trackUrl when _trackUrlRegex.IsMatch(query) => trackUrl,
                var albumUrl when _albumUrlRegex.IsMatch(query) => albumUrl,
                _ =>
                $"https://bandcamp.com/search?q={WebUtility.UrlEncode(query)}"
            };

            var json = await ScrapeJsonAsync(query)
                .ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(json))
                return response.WithStatus(SearchStatus.SearchError);


            var bcResult = JsonSerializer.Deserialize<BandCampResult>(json);
            response.WithStatus(bcResult.ItemType switch
            {
                "album" => SearchStatus.PlaylistLoaded,
                "track" => SearchStatus.TrackLoaded,
                _       => SearchStatus.NoMatches
            });

            if (response.Status == SearchStatus.NoMatches)
                return response;

            long duration = 0;
            foreach (var trackInfo in bcResult.TrackInfo)
            {
                var track = trackInfo.AsTrackInfo(bcResult.Artist, bcResult.Url, bcResult.ArtId);
                response.WithTrack(track);
                duration += track.Duration;
            }

            var playlistInfo = new PlaylistInfo()
                .WithId($"{bcResult.Current.Id}")
                .WithName(bcResult.Current.Title)
                .WithUrl(bcResult.Url)
                .WithDuration(duration)
                .WithArtwork(bcResult.ArtId == 0 ? default : $"https://f4.bcbits.com/img/a{bcResult.ArtId}_0.jpg");

            response.WithPlaylist(playlistInfo);

            return response;
        }

        /// <inheritdoc />
        public readonly async ValueTask<Stream> GetStreamAsync(string trackId)
        {
            if (!_trackUrlRegex.IsMatch(trackId))
                return default;

            var json = await ScrapeJsonAsync(trackId)
                .ConfigureAwait(false);
            var bcResult = JsonSerializer.Deserialize<BandCampResult>(json);

            var track = bcResult.TrackInfo.FirstOrDefault();

            if (track.Equals(default(BandCampTrack)))
                throw new Exception("Failed to fetch stream.");

            var stream = await _restClient
                .GetStreamAsync(track.File.Mp3Url)
                .ConfigureAwait(false);

            return stream;
        }

        /// <inheritdoc />
        public readonly ValueTask<Stream> GetStreamAsync(TrackInfo track)
            => GetStreamAsync(track.Id);

        private readonly async ValueTask<string> ScrapeJsonAsync(string url)
        {
            var rawHtml = await _restClient
                .GetStringAsync(url)
                .ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(rawHtml))
                return string.Empty;

            const string startStr = "var TralbumData = {",
                         endStr = "};";

            if (rawHtml.IndexOf(startStr, StringComparison.Ordinal) == -1)
                return string.Empty;

            var tempData = rawHtml.Substring(rawHtml.IndexOf(startStr, StringComparison.Ordinal) + startStr.Length - 1);
            tempData = tempData.Substring(0, tempData.IndexOf(endStr, StringComparison.Ordinal) + 1);

            var jsonReg = new Regex(@"([a-zA-Z0-9_]*:\s)(?!\s)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var commentReg = new Regex(@"\/\*[\s\S]*?\*\/|([^:]|^)\/\/.*",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

            tempData = commentReg.Replace(tempData, "");
            var matches = jsonReg.Matches(tempData);
            foreach (Match match in matches)
            {
                var val = $"\"{match.Value.Replace(": ", "")}\":";
                var regex = new Regex(Regex.Escape(match.Value), RegexOptions.Compiled | RegexOptions.IgnoreCase);
                tempData = regex.Replace(tempData, val, 1);
            }

            tempData = tempData.Replace("\" + \"", "");
            return tempData;
        }
    }
}