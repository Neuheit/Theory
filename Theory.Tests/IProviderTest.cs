using System.Threading.Tasks;

namespace Theory.Tests
{
    public interface IProviderTest
    {
        Task PerformSearchAsync(string query);

        Task GetPlaylistAsync(string playlistUrl);

        Task GetTrackAsync(string trackUrl);

        Task GetStreamAsync(string trackUrl);
    }
}