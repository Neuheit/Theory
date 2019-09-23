using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Theory.Providers.SoundCloud.Entities
{
    internal struct SoundCloudIdHelper : IDisposable
    {
        private const string BASE_URL = "https://soundcloud.com";

        private readonly Regex PAGE_SCRIPT_REGEX;
        private readonly Regex SCRIPT_CLIENT_ID_REGEX;
        private readonly CancellationTokenSource _cts;
        private readonly HttpClient _httpClient;

        private bool IsDisposed { get; set; }

        private string CLIENT_ID { get; set; }

        private DateTime LastIdGet { get; set; }

        internal SoundCloudIdHelper(int _)
        {
            PAGE_SCRIPT_REGEX = new Regex("https://[A-Za-z0-9-.]+/assets/app-[a-f0-9-]+\\.js", RegexOptions.Compiled);
            SCRIPT_CLIENT_ID_REGEX = new Regex(",client_id:\"([a-zA-Z0-9-_]+)\"", RegexOptions.Compiled);
            _cts = new CancellationTokenSource();
            IsDisposed = false;
            CLIENT_ID = "a3dd183a357fcff9a6943c0d65664087";
            LastIdGet = new DateTime(1980, 1, 1);
            _httpClient = new HttpClient();
        }

        internal async ValueTask<string> GetIdAsync()
        {
            if (LastIdGet.AddHours(1) < DateTime.Now)
                return await GenerateNewIdAsync().ConfigureAwait(false);

            return CLIENT_ID;
        }

        internal ValueTask<string> ForceIdUpdateAsync()
            => GenerateNewIdAsync();

        private async ValueTask<string> GenerateNewIdAsync()
        {
            if (!IsDisposed && !_cts.IsCancellationRequested)
            {
                var scriptUrl = await GetScriptUrlAsync().ConfigureAwait(false);
                var id = await GetIdFromScriptAsync(scriptUrl).ConfigureAwait(false);

                if (!string.IsNullOrWhiteSpace(id))
                {
                    LastIdGet = DateTime.Now;
                    CLIENT_ID = id;
                    return id;
                }
                else
                    throw new Exception("The script page returns empty id.");
            }
            throw new ObjectDisposedException(nameof(SoundCloudIdHelper));
        }

        private async ValueTask<string> GetScriptUrlAsync()
        {
            var page = await GetStringAsync(BASE_URL).ConfigureAwait(false);

            if (PAGE_SCRIPT_REGEX.IsMatch(page))
            {
                var match = PAGE_SCRIPT_REGEX.Match(page);

                var scriptUrl = match.Value;

                return scriptUrl;
            }
            else
            {
                throw new Exception("Failed to get script url from page.");
            }
        }

        private async ValueTask<string> GetIdFromScriptAsync(string scriptUrl)
        {
            var scriptPage = await GetStringAsync(scriptUrl);
            if (SCRIPT_CLIENT_ID_REGEX.IsMatch(scriptPage))
            {
                var match = SCRIPT_CLIENT_ID_REGEX.Match(scriptPage);
                return match.Groups[1].Value;
            }
            throw new Exception("Failed to get client id from script page.");
        }

        //If it's not a Task the Thread don't wait the _httpClient.SendAsync() completes;
        private async Task<string> GetStringAsync(string url)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url));

            request.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml");
            request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
            request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
            request.Headers.TryAddWithoutValidation("Accept-Charset", "ISO-8859-1");

            using var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(response.ReasonPhrase);

            using var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

            using var decompressedStream = new GZipStream(responseStream, CompressionMode.Decompress);

            using var streamReader = new StreamReader(decompressedStream);

            return await streamReader.ReadToEndAsync().ConfigureAwait(false);
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                _cts.Cancel();
                _cts.Dispose();
                _httpClient.Dispose();
                IsDisposed = true;
            }
        }
    }
}