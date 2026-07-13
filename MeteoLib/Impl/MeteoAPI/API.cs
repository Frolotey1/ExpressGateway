using MeteoLib.AviationWeather;
using MeteoLib.Interfaces;
using static MeteoLib.SourceText;

namespace MeteoLib.Impl.MeteoAPI
{
    public class API : IMeteoAPI
    {
        private readonly IAWC _awc;
        private readonly IAirportDictionary _airportDictionary;

        public API(IAWC awc, IAirportDictionary airportDictionary)
        {
            _awc = awc;
            _airportDictionary = airportDictionary;
        }

        public async Task<MeteoRecord> MetarAsync(string iata, Language language = Language.RUS)
        {
            var icao = await _airportDictionary.GetICAOAsync(iata);
            


            var m = _awc.MetarData(icao);

            if (m == null)
                throw new MeteoEmptyException(icao);


            return new MeteoRecordFactory(language).CreateFrom(m);


        }
        public async Task<TafMeteoRecord> TafAsync(string iata, Language language = Language.RUS)
        {
            var icao = await _airportDictionary.GetICAOAsync(iata);
            var m = _awc.TafData(icao);
            if (m == null)
                throw new MeteoEmptyException(icao);
            return new MeteoRecordFactory(language).CreateFrom(m);
        }
    }
}
