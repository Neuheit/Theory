using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Theory.Infos;
using Theory.Interfaces;
using Theory.Providers.SoundCloud.Entities;
using Theory.Search;

namespace Theory.Providers.SoundCloud
{
    public readonly struct SoundCloudProvider : IAudioProvider
    {
        private const string BASE_URL = "https://api.soundcloud.com";

        private readonly RestClient _restClient;

        public SoundCloudProvider(RestClient restClient)
            => _restClient = restClient;

        /// <inheritdoc />
        public readonly async ValueTask<SearchResponse> SearchAsync(string query)
        {
            var response = SearchResponse.Create(query);
            var url = string.Empty;

            await SoundCloudHelper.ValidateClientIdAsync(_restClient)
                .ConfigureAwait(false);

            var id = SoundCloudHelper.ClientId;
            switch (query)
            {
                case var q when Uri.IsWellFormedUriString(query, UriKind.Absolute):
                    if (!q.Contains("sets"))
                    {
                        url = BASE_URL
                            .WithPath("resolve")
                            .WithParameter("url", query)
                            .WithParameter("client_id", SoundCloudHelper.ClientId);

                        response.WithStatus(SearchStatus.TrackLoaded);
                    }
                    else
                    {
                        url = BASE_URL
                            .WithPath("resolve")
                            .WithParameter("url", query)
                            .WithParameter("client_id", SoundCloudHelper.ClientId);

                        response.WithStatus(SearchStatus.PlaylistLoaded);
                    }

                    break;

                case var _ when !Uri.IsWellFormedUriString(query, UriKind.Absolute):
                    url = BASE_URL
                        .WithPath("tracks")
                        .WithParameter("q", query)
                        .WithParameter("client_id", SoundCloudHelper.ClientId);

                    response.WithStatus(SearchStatus.SearchResult);
                    break;
            }

            var bytes = await _restClient.GetBytesAsync(url)
                .ConfigureAwait(false);

            if (bytes.IsEmpty)
                return response.WithStatus(SearchStatus.NoMatches);

            switch (response.Status)
            {
                case SearchStatus.TrackLoaded:
                    var scTrack = JsonSerializer.Deserialize<SoundCloudTrack>(bytes.Span);
                    response.WithTrack(scTrack.AsTrackInfo);
                    break;

                case SearchStatus.PlaylistLoaded:
                    var scPly = JsonSerializer.Deserialize<SoundCloudPlaylist>(bytes.Span);
                    response
                        .WithPlaylist(scPly.AsPlaylistInfo)
                        .WithTracks(scPly.Tracks.Select(x => x.AsTrackInfo));

                    break;

                case SearchStatus.SearchResult:
                    var scTracks = JsonSerializer.Deserialize<IEnumerable<SoundCloudTrack>>(bytes.Span);
                    response.WithTracks(scTracks.Select(x => x.AsTrackInfo));
                    break;
            }

            return response;
        }

        /// <inheritdoc />
        public readonly async ValueTask<Stream> GetStreamAsync(string trackId)
        {
            await SoundCloudHelper.ValidateClientIdAsync(_restClient)
                .ConfigureAwait(false);

            var stream = await _restClient
                .WithUrl(BASE_URL)
                .WithPath("tracks")
                .WithPath(trackId)
                .WithPath("stream")
                .WithParameter("client_id", SoundCloudHelper.ClientId)
                .GetStreamAsync()
                .ConfigureAwait(false);

            return stream;
        }

        /// <inheritdoc />
        public readonly ValueTask<Stream> GetStreamAsync(TrackInfo track)
            => GetStreamAsync(track.Id);
    }
}