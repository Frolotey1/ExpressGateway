namespace MeteoLib.AviationWeather
{
    public interface IAWC
    {
        //[Obsolete]
        //IEnumerable<Metar> MetarData(string icao, long startTime, long endTime);
        Metar? MetarData(string icao);
        Taf? TafData(string icao);
    }
}
