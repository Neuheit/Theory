using System.Collections.Generic;
using Theory.Interfaces;
using Theory.Providers;
using Theory.Providers.SoundCloud;
using Theory.Providers.YouTube;

namespace Theory
{
    public readonly struct Theoretical
    {
        private readonly RestClient _restClient;
        private readonly IDictionary<ProviderType, IAudioSource> _audioSources;

        public readonly int Sources
            => _audioSources.Count;

        public Theoretical(int _)
        {
            _restClient = new RestClient(default);
            _audioSources = new Dictionary<ProviderType, IAudioSource>
            {
                {ProviderType.YouTube, new YouTubeSource()},
                {ProviderType.YouTube, new SoundCloudSource(_restClient)}
            };
        }

        public IAudioSource GetSource(ProviderType providerType)
            => _audioSources[providerType];
    }
}