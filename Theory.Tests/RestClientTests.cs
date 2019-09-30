using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Theory.Tests
{
    [TestClass]
    public sealed class RestClientTests
    {
        private readonly RestClient _restClient
            = new RestClient();

        [DataTestMethod]
        [DataRow("https://google.com")]
        [DataRow("https://discordapp.com/")]
        [DataRow("https://youtube.com")]
        public async Task GetStreamAsync(string url)
        {
            var stream = await _restClient.GetStreamAsync(url)
                .ConfigureAwait(false);

            Assert.IsNotNull(stream);
            Assert.IsNotNull(stream.Length);
            Assert.IsFalse(stream.Length == 0);
        }

        [DataTestMethod]
        [DataRow("https://google.com")]
        [DataRow("https://discordapp.com/")]
        [DataRow("https://youtube.com")]
        public async Task GetBytesAsync(string url)
        {
            var bytes = await _restClient.GetBytesAsync(url)
                .ConfigureAwait(false);

            Assert.IsNotNull(bytes);
            Assert.IsFalse(bytes.IsEmpty);
        }

        [DataTestMethod]
        [DataRow("https://google.com")]
        [DataRow("https://discordapp.com/")]
        [DataRow("https://youtube.com")]
        public async Task GetStringAsync(string url)
        {
            var rawString = await _restClient.GetStringAsync(url)
                .ConfigureAwait(false);

            Assert.IsNotNull(rawString);
            Assert.IsFalse(rawString.Length == 0);
        }
    }
}