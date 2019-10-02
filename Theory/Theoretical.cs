using System.Collections.Generic;
using Theory.Interfaces;
using Theory.Providers;
using Theory.Providers.BandCamp;
using Theory.Providers.Http;
using Theory.Providers.SoundCloud;
using Theory.Providers.YouTube;

namespace Theory
{
    public sealed class Theoretical
    {
        private readonly IDictionary<ProviderType, IAudioProvider> _audioProviders;

        public int Providers
            => _audioProviders.Count;

        public Theoretical()
        {
            var restClient = new RestClient();
            _audioProviders = new Dictionary<ProviderType, IAudioProvider>
            {
                {ProviderType.YouTube, new YouTubeProvider(restClient)},
                {ProviderType.SoundCloud, new SoundCloudProvider(restClient)},
                {ProviderType.BandCamp, new BandCampProvider(restClient)},
                {ProviderType.Http, new HttpProvider(restClient)}
            };
        }

        public IAudioProvider GetProvider(ProviderType providerType)
            => _audioProviders[providerType];
    }
}