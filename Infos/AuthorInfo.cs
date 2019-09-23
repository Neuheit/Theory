namespace Theory.Infos
{
    public struct AuthorInfo
    {
        public string Name { get; private set; }
        public string Url { get; private set; }
        public string AvatarUrl { get; private set; }

        public AuthorInfo WithName(string name)
        {
            Name = name;
            return this;
        }

        public AuthorInfo WithUrl(string url)
        {
            Url = url;
            return this;
        }

        public AuthorInfo WithAvatar(string avatarUrl)
        {
            AvatarUrl = avatarUrl;
            return this;
        }
    }
}