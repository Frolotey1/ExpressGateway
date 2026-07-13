using static MeteoLib.SourceText;

namespace MeteoLib.Interfaces
{
    public interface IMeteoAPI
    {
        Task<MeteoRecord> MetarAsync(string iatacode, Language language = Language.RUS);

        Task<TafMeteoRecord> TafAsync(string iatacode, Language language = Language.RUS);
    }
}