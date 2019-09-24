using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Theory.Providers.SoundCloud;
using Theory.Search;

namespace Theory.Tests
{
    [TestClass]
    public sealed class SoundCloudTests
    {
        private readonly RestClient _restClient
            = new RestClient(default);

        private SoundCloudProvider CloudProvider
            => new SoundCloudProvider(_restClient);

        [TestMethod]
        public async Task FetchIdTestAsync()
        {
            for (var i = 0; i < 5; i++)
            {
                await SoundCloudHelper.ValidateClientIdAsync(_restClient)
                    .ConfigureAwait(false);

                Assert.IsNotNull(SoundCloudHelper.ClientId);
            }
        }

        [TestMethod]
        public async Task SearchAsync()
        {
            var search = await CloudProvider.SearchAsync("The Weeknd Valerie")
                .ConfigureAwait(false);

            Assert.Equals(search.Status, SearchStatus.SearchResult);
            Assert.IsNotNull(search.Tracks);
            Assert.IsNull(search.Playlist);
        }

        [TestMethod]
        public async Task GetPlaylistAsync()
        {
            var search = await CloudProvider
                .SearchAsync("https://soundcloud.com/albsoon/sets/the-weeknd-more-balloons-remixed-by-sango")
                .ConfigureAwait(false);

            Assert.Equals(search.Status, SearchStatus.PlaylistLoaded);
            Assert.IsNotNull(search.Tracks);
            Assert.IsNotNull(search.Playlist);
        }

        [TestMethod]
        public async Task GetNormalTrackAsync()
        {
            var search = await CloudProvider
                .SearchAsync("https://soundcloud.com/albsoon/01-the-morning-sango-remix")
                .ConfigureAwait(false);

            Assert.Equals(search.Status, SearchStatus.TrackLoaded);
            Assert.IsNotNull(search.Tracks);
            Assert.IsNotNull(search.Playlist);
        }

        public async Task GetRestrictedTrackAsync()
        {
            var search = await CloudProvider
                .SearchAsync("https://soundcloud.com/theweeknd/hurt-you")
                .ConfigureAwait(false);

            Assert.Equals(search.Status, SearchStatus.TrackLoaded);
            Assert.IsNotNull(search.Tracks);
            Assert.IsNotNull(search.Playlist);
        }

        [TestMethod]
        public async Task GetStreamAsync()
        {
        }
    }
}