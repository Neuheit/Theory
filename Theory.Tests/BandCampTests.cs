using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Theory.Providers.BandCamp;
using Theory.Search;

namespace Theory.Tests
{
    [TestClass]
    public sealed class BandCampTests : IProviderTest
    {
        private readonly RestClient _restClient
            = new RestClient(default);

        private BandCampProvider CampProvider
            => new BandCampProvider(_restClient);

        [DataTestMethod]
        [DataRow("Travis Scott Through The Late Night")]
        [DataRow("Daniel Ceaser Get You")]
        public async Task PerformSearchAsync(string query)
        {
            var response = await CampProvider
                .SearchAsync(query)
                .ConfigureAwait(false);

            Assert.AreEqual(SearchStatus.SearchResult, response.Status);
            Assert.IsNotNull(response.Tracks);
            Assert.IsTrue(response.Tracks.Count > 0);
            Assert.IsNull(response.Playlist.Name);
        }

        [DataTestMethod]
        [DataRow("https://slimk4.bandcamp.com/album/astrochops")]
        [DataRow("https://pennypolice.bandcamp.com/album/be-lucky")]
        [DataRow("https://glennastro.bandcamp.com/album/naturals")]
        public async Task GetPlaylistAsync(string playlistUrl)
        {
            var response = await CampProvider
                .SearchAsync(playlistUrl)
                .ConfigureAwait(false);

            Assert.AreEqual(SearchStatus.PlaylistLoaded, response.Status);
            Assert.IsNotNull(response.Playlist);
            Assert.IsNotNull(response.Playlist.Name);
            Assert.IsTrue(response.Tracks.Count > 0);
        }

        [DataTestMethod]
        [DataRow("https://slimk4.bandcamp.com/track/sicko-mode-chopnotslop-remix")]
        [DataRow("https://ostgut.bandcamp.com/track/be-true-to-me")]
        [DataRow("https://sambarker.bandcamp.com/track/models-of-wellbeing")]
        public async Task GetTrackAsync(string trackUrl)
        {
            var response = await CampProvider
                .SearchAsync(trackUrl)
                .ConfigureAwait(false);

            Assert.AreEqual(SearchStatus.TrackLoaded, response.Status);
            Assert.IsTrue(response.Tracks.Count == 1);
        }

        [DataTestMethod]
        [DataRow("https://slimk4.bandcamp.com/track/sicko-mode-chopnotslop-remix")]
        [DataRow("https://ostgut.bandcamp.com/track/be-true-to-me")]
        [DataRow("https://sambarker.bandcamp.com/track/models-of-wellbeing")]
        public async Task GetStreamAsync(string trackUrl)
        {
            var stream = await CampProvider.GetStreamAsync(trackUrl)
                .ConfigureAwait(false);

            Assert.IsNotNull(stream);
            Assert.IsTrue(stream.Length > 0);
            Assert.IsTrue(stream.CanRead);
            Assert.IsTrue(stream.CanWrite);
        }
    }
}