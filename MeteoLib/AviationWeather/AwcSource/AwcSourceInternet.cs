using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace MeteoLib.AviationWeather.AwcSource;

public class AwcSourceInternet : IAwcSource
{
    // https://aviationweather.gov/data/api/#schema 

    public string GetMetarUrl(string icao)
    {
        var hours = 1;
        // return $"https://aviationweather.gov/cgi-bin/data/dataserver.php?requestType=retrieve&dataSource=metars&stationString={icao}&hoursBeforeNow={hours}&format=xml";
        return $"https://aviationweather.gov/api/data/metar?ids={icao}&format=xml&taf=false&hours={hours}";
    }

    public string GetTafUrl(string icao)
    {
        // var hours = 1;
        // return $"https://aviationweather.gov/cgi-bin/data/dataserver.php?requestType=retrieve&dataSource=tafs&stationString={icao}&hoursBeforeNow={hours}&format=xml";
        return $"https://aviationweather.gov/api/data/taf?ids={icao}&format=xml"; // &time=issue
    }
}
