using System;
using System.Collections.Generic;
using System.Text;

namespace Theory.Providers.YouTube.Entities
{
    internal readonly struct YouTubePlayerConfig
    {
        public readonly string PlayerSourceUrl;

        public readonly string StreamsInfoUrlEncoded;

        public readonly string DashManifestUrl;

        internal YouTubePlayerConfig(
            string playerSourceUrl,
            string streamsInfoUrlEncoded,
            string dashManifestUrl)
        {
            PlayerSourceUrl = playerSourceUrl;
            StreamsInfoUrlEncoded = streamsInfoUrlEncoded;
            DashManifestUrl = dashManifestUrl;
        }
    }
}