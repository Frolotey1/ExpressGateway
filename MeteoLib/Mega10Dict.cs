using Daocore.OracleEngine;
using MeteoLib.Interfaces;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Concurrent;

namespace MeteoLib
{
    public class Mega10Dict : IAirportDictionary
    {
        private readonly ILogger _logger;
        private readonly IConnectionFactory _connectionFactory;
        private readonly ConcurrentDictionary<string, string> _airportCodeDictionary = new();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public Mega10Dict(ILogger<Mega10Dict> logger, IConnectionFactory connectionFactory)
        {
            _logger = logger;
            _connectionFactory = connectionFactory;
        }

        public async Task<string> GetICAOAsync(string iata)
        {
            var cached = _airportCodeDictionary.ContainsKey(iata);

            

            if (cached)
            {
                
                return _airportCodeDictionary[iata];
            }
            else
            {
                await _semaphore.WaitAsync();
                try
                { 
                    using var c = _connectionFactory.CreateMegaConnection();

                    var t = new TableAdapter("nsi.dict_ap");

                    var icao = t.Query<string>(c, "code", "iata", iata.ToUpper());

                    if (string.IsNullOrEmpty(icao)) throw new Exception("EMPTY ICAO!");

                    _airportCodeDictionary.TryAdd(iata, icao);

                    return icao;
                }
                finally
                {
                    _semaphore.Release();
                }
            }





        }

    }
}
