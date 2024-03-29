﻿using System;
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
            if (string.IsNullOrWhiteSpace(url))
                throw new Exception($"url param cannot be null or empty.");

            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                throw new Exception("url param needs to be a url.");

            using var pageHeaders = await _restClient
                                        .WithUrl(url)
                                        .HeadAsync()
                                        .ConfigureAwait(false);

            if (!pageHeaders.Content.Headers.ContentType.MediaType.Contains("audio"))
                throw new Exception("url param needs to be a audio url.");

            var stream = await _restClient
                            .WithUrl(url)
                            .GetStreamAsync()
                            .ConfigureAwait(false);

            return stream;
        }

        // I think is better throw a exception here lol.
        public ValueTask<Stream> GetStreamAsync(TrackInfo track)
            => throw new NotSupportedException();
    }
}