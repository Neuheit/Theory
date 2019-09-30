using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Theory.Providers.YouTube;
using Theory.Search;

namespace Theory.Tests
{
    [TestClass]
    public sealed class YouTubeTests : IProviderTest
    {
        private readonly RestClient _restClient
            = new RestClient();

        private YouTubeProvider TubeProvider
            => new YouTubeProvider(_restClient);

        [DataTestMethod]
        [DataRow("Travis Scott Through The Late Night")]
        [DataRow("Daniel Ceaser Get You")]
        [DataRow("The Weeknd Call Out My Name")]
        public async Task PerformSearchAsync(string query)
        {
            var response = await TubeProvider.SearchAsync(query)
                .ConfigureAwait(false);

            Assert.AreEqual(SearchStatus.SearchResult, response.Status);
            Assert.IsNotNull(response.Tracks);
            Assert.IsTrue(response.Tracks.Count > 0);
            Assert.IsNull(response.Playlist.Name);
        }

        [DataTestMethod]
        [DataRow("https://www.youtube.com/playlist?list=PL8Go-YHXcY4bnSo0BDjYt9KuW9izuBD8U")]
        [DataRow("https://www.youtube.com/playlist?list=OLAK5uy_nrbHEhhkZIpj3-XdSQm75NdaqxRScGpQc&playnext=1&index=1")]
        public async Task GetPlaylistAsync(string playlistUrl)
        {
            var response = await TubeProvider.SearchAsync(playlistUrl)
                .ConfigureAwait(false);

            Assert.AreEqual(SearchStatus.PlaylistLoaded, response.Status);
            Assert.IsNotNull(response.Playlist);
            Assert.IsNotNull(response.Playlist.Name);
            Assert.IsNotNull(response.Tracks);
            Assert.IsTrue(response.Tracks.Count > 0);
        }

        [DataTestMethod]
        [DataRow("https://www.youtube.com/watch?v=z7kYa5GQYKg")]
        [DataRow("https://youtu.be/h5zkDVhR2Vk")]
        public async Task GetTrackAsync(string trackUrl)
        {
            var response = await TubeProvider.SearchAsync(trackUrl)
                .ConfigureAwait(false);

            Assert.AreEqual(SearchStatus.TrackLoaded, response.Status);
            Assert.IsTrue(response.Tracks.Count == 1);
        }

        [DataTestMethod]
        [DataRow("https://www.youtube.com/watch?v=z7kYa5GQYKg")]
        [DataRow("https://youtu.be/h5zkDVhR2Vk")]
        public async Task GetStreamAsync(string trackUrl)
        {
            var stream = await TubeProvider.GetStreamAsync(trackUrl);

            Assert.IsNotNull(stream);
            Assert.IsFalse(stream.Length == 0);
        }
    }
}