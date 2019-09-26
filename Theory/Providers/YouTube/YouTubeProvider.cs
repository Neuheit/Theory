using System;
using System.Collections.Generic;
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
    public readonly struct YouTubeProvider : IAudioProvider
    {
        private const string BASE_URL = "https://www.youtube.com";

        private readonly RestClient _restClient;

        public YouTubeProvider(RestClient restClient)
           => _restClient = restClient;

        /// <inheritdoc />
        public readonly async ValueTask<SearchResponse> SearchAsync(string query)
        {
            var response = SearchResponse.Create(query);
            string url;
            YouTubeParser.ParseId(query, out var videoId, out var playlistId);

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
                            .WithParameter("search_query", WebUtility.UrlEncode(query))
                            .WithParameter("hl", "en");

                        response.WithStatus(SearchStatus.TrackLoaded);
                    }

                    break;

                case false:
                    url = BASE_URL
                        .WithPath("search_ajax")
                        .WithParameter("style", "json")
                        .WithParameter("search_query", WebUtility.UrlEncode(query))
                        .WithParameter("hl", "en");

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
        {
            var stream = new MemoryStream();
            using var audioTrackStream = await YouTubeTrackLoader.LoadTrackAsync(trackId).ConfigureAwait(false);
            await audioTrackStream.CopyToAsync(stream).ConfigureAwait(false);
            return stream;
        }

        /// <inheritdoc />
        public readonly ValueTask<Stream> GetStreamAsync(TrackInfo track)
            => GetStreamAsync(track.Id);
    }
}