using System.Threading.Tasks;

namespace Theory.Tests
{
    public interface IProviderTest
    {
        Task PerformSearchAsync(string query);

        Task GetPlaylistAsync(string playlistLink);

        Task GetTrackAsync(string trackLink);
    }
}