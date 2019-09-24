using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Theory.Providers.SoundCloud;
using Theory.Search;

namespace Theory.Tests
{
    [TestClass]
    public sealed class SoundCloudTests : IProviderTest
    {
        private readonly RestClient _restClient
            = new RestClient(default);

        private SoundCloudProvider CloudProvider
            => new SoundCloudProvider(_restClient);

        [DataTestMethod]
        [DataRow("Travis Scott Through The Late Night")]
        [DataRow("Daniel Ceaser Get You")]
        [DataRow("The Weeknd Call Out My Name")]
        public async Task PerformSearchAsync(string query)
        {
            var response = await CloudProvider.SearchAsync(query)
                .ConfigureAwait(false);

            Assert.AreEqual(SearchStatus.SearchResult, response.Status);
            Assert.IsNotNull(response.Tracks);
            Assert.IsTrue(response.Tracks.Count > 0);
            Assert.IsNull(response.Playlist.Name);
        }

        [DataTestMethod]
        [DataRow("https://soundcloud.com/kanyewest/sets/ye-49")]
        [DataRow("https://soundcloud.com/albsoon/sets/the-weeknd-more-balloons-remixed-by-sango")]
        [DataRow("https://soundcloud.com/travisscott-2/sets/astroworld")]
        public async Task GetPlaylistAsync(string playlistLink)
        {
            var response = await CloudProvider.SearchAsync(playlistLink)
                .ConfigureAwait(false);

            Assert.AreEqual(SearchStatus.PlaylistLoaded, response.Status);
            Assert.IsNotNull(response.Playlist);
            Assert.IsNotNull(response.Playlist.Name);
            Assert.IsTrue(response.Tracks.Count > 0);
        }

        [DataTestMethod]
        [DataRow("https://soundcloud.com/theweeknd/hurt-you")]
        [DataRow("https://soundcloud.com/gesaffelstein/lost-in-the-fire")]
        [DataRow("https://soundcloud.com/kanyewest/i-love-it-freaky-girl-edit")]
        public async Task GetTrackAsync(string trackLink)
        {
            var response = await CloudProvider.SearchAsync(trackLink)
                .ConfigureAwait(false);

            Assert.AreEqual(SearchStatus.TrackLoaded, response.Status);
            Assert.IsTrue(response.Tracks.Count == 1);
        }

        [TestMethod]
        public async Task FetchIdTestAsync()
        {
            await SoundCloudHelper.ValidateClientIdAsync(_restClient)
                .ConfigureAwait(false);

            Assert.IsNotNull(SoundCloudHelper.ClientId);
        }

        [DataTestMethod]
        [DataRow("https://soundcloud.com/theweeknd/hurt-you")]
        [DataRow("https://soundcloud.com/gesaffelstein/lost-in-the-fire")]
        [DataRow("https://soundcloud.com/kanyewest/i-love-it-freaky-girl-edit")]
        public async Task GetStreamAsync(string streamUrl)
        {
            var search = await CloudProvider
                .SearchAsync(streamUrl)
                .ConfigureAwait(false);

            var track = search.Tracks.FirstOrDefault();
            Assert.IsNotNull(track);

            var stream = await CloudProvider.GetStreamAsync(track)
                .ConfigureAwait(false);

            Assert.IsNotNull(stream);
            Assert.IsFalse(stream.Length == 0);
        }
    }
}