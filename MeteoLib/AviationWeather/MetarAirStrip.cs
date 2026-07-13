using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteoLib.AviationWeather
{
#pragma warning disable IDE1006 // Стили именования

    public class MetarAirStrip
    {
        public string air_strip_code { get; set; } = string.Empty;
        public double adhesion { get; set; }
        public string adhesionrow { get; set; } = string.Empty;
    }


#pragma warning restore IDE1006 // Стили именования
}
