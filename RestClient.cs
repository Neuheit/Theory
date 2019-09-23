using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Theory
{
    public struct RestClient
    {
        private string _url;
        private readonly HttpClient _client;

        public RestClient(IWebProxy proxy = default)
        {
            _url = default;
            _client = new HttpClient(new HttpClientHandler
            {
                Proxy = proxy,
                UseCookies = false,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            });
        }

        public RestClient WithUrl(string url)
        {
            _url = url;
            return this;
        }

        public RestClient WithPath(string path)
        {
            _url = _url.WithPath(path);
            return this;
        }

        public RestClient WithParameter(string key, string value)
        {
            _url = _url.WithParameter(key, value);
            return this;
        }

        public async ValueTask<ReadOnlyMemory<byte>> GetBytesAsync(string url = default)
        {
            url ??= _url;

            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(url);

            using var get = await _client.GetAsync(url)
                .ConfigureAwait(false);

            if (!get.IsSuccessStatusCode)
                throw new HttpRequestException();

            using var content = get.Content;
            var array = await content.ReadAsByteArrayAsync()
                .ConfigureAwait(false);

            _url = string.Empty;
            return array;
        }

        public async ValueTask<Stream> GetStreamAsync(string url = default)
        {
            url ??= _url;

            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(url);

            using var get = await _client.GetAsync(url)
                .ConfigureAwait(false);

            if (!get.IsSuccessStatusCode)
                throw new HttpRequestException();

            using var content = get.Content;
            var stream = await content.ReadAsStreamAsync()
                .ConfigureAwait(false);

            _url = string.Empty;
            return stream;
        }

        public async ValueTask<string> GetStringAsync(string url = default)
        {
            url ??= _url;

            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(url);

            using var get = await _client.GetAsync(url)
                .ConfigureAwait(false);

            if (!get.IsSuccessStatusCode)
                throw new HttpRequestException();

            using var content = get.Content;
            var str = await content.ReadAsStringAsync()
                .ConfigureAwait(false);

            _url = string.Empty;
            return str;
        }
    }
}