using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Theory
{
    public struct RestClient
    {
        private string _url;
        private readonly HttpClient _client;
        private long? _maxRange;

        public RestClient(IWebProxy proxy = default)
        {
            _url = default;
            _client = new HttpClient(new HttpClientHandler
            {
                Proxy = proxy,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            });
            _maxRange = default;
        }

        public RestClient WithRange(long maxRange)
        {
            _maxRange = maxRange;
            return this;
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

        public RestClient WithHeader(string key, string value)
        {
            if (_client.DefaultRequestHeaders.Contains(key))
                return this;

            _client.DefaultRequestHeaders.Add(key, value);
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
                throw new HttpRequestException(get.ReasonPhrase);

            using var content = get.Content;
            var array = await content.ReadAsByteArrayAsync()
                .ConfigureAwait(false);

            _url = string.Empty;
            _client.DefaultRequestHeaders.Clear();
            return array;
        }

        public async ValueTask<Stream> GetStreamAsync(string url = default, long? maxRange = default)
        {
            url ??= _url;
            maxRange ??= _maxRange;

            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(url);

            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            if (maxRange.HasValue)
                request.Headers.Range = new RangeHeaderValue(0, maxRange);

            using var get = await _client.SendAsync(request)
                .ConfigureAwait(false);

            if (!get.IsSuccessStatusCode)
                throw new HttpRequestException(get.ReasonPhrase);

            using var stream = await get.Content.ReadAsStreamAsync().ConfigureAwait(false);

            var ms = new MemoryStream();
            await stream.CopyToAsync(ms).ConfigureAwait(false);

            _url = string.Empty;
            _client.DefaultRequestHeaders.Clear();

            return ms;
        }

        public async ValueTask<string> GetStringAsync(string url = default)
        {
            url ??= _url;

            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(url);

            using var get = await _client.GetAsync(url)
                .ConfigureAwait(false);

            if (!get.IsSuccessStatusCode)
                throw new HttpRequestException(get.ReasonPhrase);

            using var content = get.Content;
            var str = await content.ReadAsStringAsync()
                .ConfigureAwait(false);

            _url = string.Empty;
            _client.DefaultRequestHeaders.Clear();

            return str;
        }

        public async ValueTask<long?> GetContentLengthAsync(string url)
        {
            using var request = new HttpRequestMessage(HttpMethod.Head, url);

            using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                .ConfigureAwait(false);

            var contentLength = response.Content.Headers.ContentLength;

            return contentLength;
        }

        public async ValueTask<HttpResponseMessage> GetAsync(string url)
        {
            return await _client.GetAsync(url)
                .ConfigureAwait(false);
        }
    }
}