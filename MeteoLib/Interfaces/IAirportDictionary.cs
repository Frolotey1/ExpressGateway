using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteoLib.Interfaces
{
    public interface IAirportDictionary
    {
        Task<string>  GetICAOAsync(string iata);
    }
}
