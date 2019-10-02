using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Theory.Infos;
using Theory.Interfaces;
using Theory.Search;

namespace Theory.Providers.Http
{
    public readonly struct HttpProvider : IAudioProvider
    {
        private readonly RestClient _restClient;

        public HttpProvider(RestClient restClient)
            => _restClient = restClient;

        public ValueTask<SearchResponse> SearchAsync(string query)
            => throw new NotSupportedException();

        public async ValueTask<Stream> GetStreamAsync(string url)
        {
            var stream = await _restClient
                            .WithUrl(url)
                            .GetStreamAsync()
                            .ConfigureAwait(false);

            return stream;
        }

        public ValueTask<Stream> GetStreamAsync(TrackInfo track)
            => GetStreamAsync(track.Url);
    }
}