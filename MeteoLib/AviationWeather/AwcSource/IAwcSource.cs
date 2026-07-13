using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteoLib.AviationWeather.AwcSource;

public interface IAwcSource
{
    string GetMetarUrl(string icao);
    string GetTafUrl(string icao);
}
