using System.IO;
using System.Threading.Tasks;
using Theory.Infos;
using Theory.Interfaces;
using Theory.Search;

namespace Theory.Providers.YouTube
{
    public readonly struct YouTubeSource : IAudioSource
    {
        /// <inheritdoc />
        public async ValueTask<SearchResponse> SearchAsync(string query)
            => default;

        /// <inheritdoc />
        public async ValueTask<Stream> GetStreamAsync(string trackId)
            => null;

        /// <inheritdoc />
        public async ValueTask<Stream> GetStreamAsync(TrackInfo track)
            => null;
    }
}