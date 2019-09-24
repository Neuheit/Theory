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

            Assert.IsNotNull(stream, "stream != null");
            Assert.IsNotNull(stream.Length, "stream.Length != null");
        }
    }
}