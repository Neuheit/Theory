using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Theory.Infos;
using Theory.Interfaces;
using Theory.Providers.YouTube.Entities;
using Theory.Search;

namespace Theory.Providers.YouTube
{
    public readonly struct YouTubeSource : IAudioSource
    {
        private const string BASE_URL = "https://www.youtube.com";
        private readonly Regex _idRegex;
        private readonly RestClient _restClient;

        public YouTubeSource(RestClient restClient)
        {
            _restClient = restClient;
            _idRegex = new Regex("(?!videoseries)[a-zA-Z0-9_-]{11,42}",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        /// <inheritdoc />
        public readonly async ValueTask<SearchResponse> SearchAsync(string query)
        {
            var response = SearchResponse.Create(query);
            string url;
            ParseId(query, out var videoId, out var playlistId);

            switch (Uri.IsWellFormedUriString(query, UriKind.Absolute))
            {
                case true:
                    if (query.Contains("list="))
                    {
                        url = BASE_URL
                            .WithPath("list_ajax")
                            .WithParameter("style", "json")
                            .WithParameter("action_get_list", "1")
                            .WithParameter("list", playlistId);

                        response.WithStatus(SearchStatus.PlaylistLoaded);
                    }
                    else
                    {
                        url = BASE_URL
                            .WithPath("search_ajax")
                            .WithParameter("style", "json")
                            .WithParameter("search_query", WebUtility.UrlEncode(query));

                        response.WithStatus(SearchStatus.TrackLoaded);
                    }

                    break;

                case false:
                    url = BASE_URL
                        .WithPath("search_ajax")
                        .WithParameter("style", "json")
                        .WithParameter("search_query", WebUtility.UrlEncode(query));

                    response.WithStatus(SearchStatus.SearchResult);
                    break;
            }

            var request = await _restClient.GetBytesAsync(url)
                .ConfigureAwait(false);

            if (request.IsEmpty)
                return response.WithStatus(SearchStatus.NoMatches);

            switch (response.Status)
            {
                case SearchStatus.PlaylistLoaded:
                    var playlist = JsonSerializer.Deserialize<YouTubePlaylist>(request.Span);
                    response.WithPlaylist(playlist.BuildPlaylistInfo(playlistId, url));
                    response.WithTracks(playlist.Videos.Select(x => x.AsTrackInfo));
                    break;

                case SearchStatus.SearchResult:
                    var ytSearch = JsonSerializer.Deserialize<YouTubeSearch>(request.Span);
                    response.WithTracks(ytSearch.Video.Select(x => x.AsTrackInfo));
                    break;

                case SearchStatus.TrackLoaded:
                    ytSearch = JsonSerializer.Deserialize<YouTubeSearch>(request.Span);
                    var track = ytSearch.Video.Select(x => x.AsTrackInfo)
                        .FirstOrDefault(x => x.Id == videoId);
                    response.WithTrack(track);
                    break;
            }

            return response;
        }

        /// <inheritdoc />
        public readonly async ValueTask<Stream> GetStreamAsync(string trackId)
            => null;

        /// <inheritdoc />
        public readonly ValueTask<Stream> GetStreamAsync(TrackInfo track)
            => GetStreamAsync(track.Id);

        private readonly void ParseId(string url, out string videoId, out string playlistId)
        {
            var matches = _idRegex.Matches(url);
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
    }
}