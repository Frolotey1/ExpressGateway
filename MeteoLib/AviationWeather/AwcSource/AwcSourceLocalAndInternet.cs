using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteoLib.AviationWeather.AwcSource;

public class AwcSourceLocalAndInternet : IAwcSource
{
    public string GetMetarUrl(string icao)
    {
        if (AwcSourceLocal.MetarTurnedOn) return AwcSourceLocal.MetarUrl;

        return new AwcSourceInternet().GetMetarUrl(icao);
    }

    public string GetTafUrl(string icao)
    {
        if (AwcSourceLocal.TafTurnedOn) return AwcSourceLocal.TafUrl;

        return new AwcSourceInternet().GetTafUrl(icao);
    }
}
