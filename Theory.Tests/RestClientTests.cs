using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Theory.Tests
{
    [TestClass]
    public class RestClientTests
    {
        private readonly RestClient _restClient
            = new RestClient(default);

        [TestMethod]
        public async Task MakeRequestsAsync()
        {
        }

        [TestMethod]
        public async Task GetStreamAsync()
        {
            var stream = await _restClient.GetStreamAsync("https://google.com")
                .ConfigureAwait(false);

            Assert.IsNotNull(stream);
            Assert.IsNotNull(stream.Length);
            Assert.IsFalse(stream.Length == 0);
        }

        [TestMethod]
        public async Task GetBytesAsync()
        {
            var bytes = await _restClient.GetBytesAsync("https://google.com")
                .ConfigureAwait(false);

            Assert.IsNotNull(bytes);
            Assert.IsFalse(bytes.IsEmpty);
        }

        [TestMethod]
        public async Task GetStringAsync()
        {
            var rawString = await _restClient.GetStringAsync("https://google.com")
                .ConfigureAwait(false);

            Assert.IsNotNull(rawString);
            Assert.IsFalse(rawString.Length == 0);
        }
    }
}