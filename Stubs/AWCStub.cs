using MeteoLib.AviationWeather;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stubs;

internal class AWCStub : IAWC
{
    public IEnumerable<Metar> MetarData(string icao, long startTime, long endTime)
    {
        yield return NewMetar();
    }

    private Metar NewMetar()
    {
        return new Metar();
    }

    public Metar? MetarData(string icao)
    {
        return NewMetar();
    }

    public Taf? TafData(string icao)
    {
        throw new NotImplementedException();
    }
}
