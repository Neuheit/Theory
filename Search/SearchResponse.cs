using System.Collections.Generic;
using System.Linq;
using Theory.Infos;

namespace Theory.Search
{
    public struct SearchResponse
    {
        public string Query { get; }
        public SearchStatus Status { get; private set; }
        public PlaylistInfo Playlist { get; private set; }
        public IEnumerable<TrackInfo> Tracks { get; private set; }

        private SearchResponse(string query)
        {
            Query = query;
            Status = SearchStatus.NoMatches;
            Playlist = default;
            Tracks = default;
        }

        public static SearchResponse Create(string query)
            => new SearchResponse(query);

        public SearchResponse WithStatus(SearchStatus status)
        {
            Status = status;
            return this;
        }

        public SearchResponse WithPlaylist(PlaylistInfo playlistInfo)
        {
            Playlist = playlistInfo;
            return this;
        }

        public SearchResponse WithTracks(IEnumerable<TrackInfo> tracks)
        {
            Tracks = tracks;
            return this;
        }

        public SearchResponse WithTrack(TrackInfo track)
        {
            Tracks = new[] {track};
            return this;
        }

        /// <inheritdoc />
        public override string ToString()
            => $"Searched for: {Query}\nStatus: {Status}" +
               $"\nPlaylist? {(Playlist.Equals(default(PlaylistInfo)) ? "NULL" : Playlist.Name)}" +
               $"\nTracks: {Tracks?.Count()}";
    }
}