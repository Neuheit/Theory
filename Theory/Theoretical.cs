using System.Collections.Generic;
using System.Net;
using Theory.Interfaces;
using Theory.Providers;
using Theory.Providers.BandCamp;
using Theory.Providers.SoundCloud;
using Theory.Providers.YouTube;

namespace Theory
{
    public readonly struct Theoretical
    {
        private readonly IDictionary<ProviderType, IAudioProvider> _audioSources;

        public readonly int Sources
            => _audioSources.Count;

        public Theoretical(IWebProxy proxy)
        {
            var restClient = new RestClient(proxy);
            _audioSources = new Dictionary<ProviderType, IAudioProvider>
            {
                {ProviderType.YouTube, new YouTubeProvider(restClient)},
                {ProviderType.SoundCloud, new SoundCloudProvider(restClient)},
                {ProviderType.BandCamp, new BandCampProvider(restClient)}
            };
        }

        public IAudioProvider GetSource(ProviderType providerType)
            => _audioSources[providerType];
    }
}