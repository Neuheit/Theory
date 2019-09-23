using System.Collections.Generic;
using System.Net;
using Theory.Interfaces;
using Theory.Providers;
using Theory.Providers.SoundCloud;
using Theory.Providers.YouTube;

namespace Theory
{
    public readonly struct Theoretical
    {
        private readonly IDictionary<ProviderType, IAudioSource> _audioSources;

        public readonly int Sources
            => _audioSources.Count;

        public Theoretical(IWebProxy proxy)
        {
            var restClient = new RestClient(proxy);
            _audioSources = new Dictionary<ProviderType, IAudioSource>
            {
                {ProviderType.YouTube, new YouTubeSource()},
                {ProviderType.SoundCloud, new SoundCloudSource(restClient)}
            };
        }

        public IAudioSource GetSource(ProviderType providerType)
            => _audioSources[providerType];
    }
}