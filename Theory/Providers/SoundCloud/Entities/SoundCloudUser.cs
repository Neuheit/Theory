using System.Text.Json.Serialization;
using Theory.Infos;

namespace Theory.Providers.SoundCloud.Entities
{
    public sealed class SoundCloudUser
    {
        [JsonPropertyName("permalink_url")]
        public string PermalinkUrl { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("avatar_url")]
        public string AvatarUrl { get; set; }

        [JsonIgnore]
        public AuthorInfo AsAuthorInfo
            => new AuthorInfo()
                .WithName(Username)
                .WithUrl(PermalinkUrl)
                .WithUrl(AvatarUrl);
    }
}