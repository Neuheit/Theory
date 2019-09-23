using System.IO;
using System.Threading.Tasks;
using Theory.Infos;
using Theory.Search;

namespace Theory.Interfaces
{
    public interface IAudioSource
    {
        ValueTask<SearchResponse> SearchAsync(string query);

        ValueTask<Stream> GetStreamAsync(string trackId);

        ValueTask<Stream> GetStreamAsync(TrackInfo track);
    }
}