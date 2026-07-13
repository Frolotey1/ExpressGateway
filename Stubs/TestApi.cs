using MeteoLib;
using MeteoLib.Interfaces;
using System.Threading.Tasks;
using static MeteoLib.SourceText;

namespace Stubs;

internal class TestApi : IMeteoAPI
{
    public async Task<MeteoRecord> MetarAsync(string iatacode, Language language = Language.RUS)
    {
        return new MeteoRecord();
    }
    public async Task<TafMeteoRecord> TafAsync(string iatacode, Language language = Language.RUS)
    {
        return new TafMeteoRecord();
    }
}
