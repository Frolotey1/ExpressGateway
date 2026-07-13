using MeteoLib;
using MeteoLib.Interfaces;
using Microsoft.Extensions.Logging;
using static MeteoLib.SourceText;

namespace MeteoLib.LoadService
{


    public class LoadService
    {
        private readonly IMeteoAPI _api;
        private readonly IRepo _rep;
        private readonly ILogger _logger;


        public LoadService(IMeteoAPI api, IRepo rep, ILogger<LoadService> logger)
        {
            _api = api;
            _rep = rep;
            _logger = logger;
        }




        public TimeSpan Delay { get; set; } = TimeSpan.FromSeconds(60);


        public async Task LoadTafAsync(string iata)
        {
            _logger.LogInformation("load taf {Iata}", iata);

            try
            {
                var current = await _api.TafAsync(iata);
                var current_eng = await _api.TafAsync(iata, Language.ENG);

                _logger.LogInformation("{dt} {asset} save taf time {time}", DateTime.UtcNow, iata, current.IssueTime);
                _rep.SaveTaf(iata, current, current_eng);
                _logger.LogInformation("{dt} {asset} saved taf time {time}", DateTime.UtcNow, iata, current.IssueTime);
            }
            catch (MeteoEmptyException)
            {
                _logger.LogWarning("no taf data {code}", iata);
                return;
            }
            catch (Exception e)
            {
                _logger.LogWarning("{code} Error. {message}", iata, e.ToString());
                return;
            }
        }
        public async Task LoadMetarAsync(string iata)
        {
            _logger.LogInformation("Load metar {iata}", iata);

            try
            {
                var current = await  _api.MetarAsync(iata);
                var current_eng = await _api.MetarAsync(iata, Language.ENG);
                var rpack = _rep.GetPacket(iata);
                var previous = rpack?.CurrentMeteo;

                _logger.LogInformation("{dt} {asset} save metar time? {time} {time}", DateTime.UtcNow, iata, previous.DateTime, current.DateTime);



                if (DifferentTimes(previous, current))
                {
                    var trigger = TriggerBuilder.Detect(previous, current);
                    _logger.LogInformation("{dt} {asset} save metar  time {time}, trigger {delivery}", DateTime.UtcNow, iata, current.DateTime, trigger.DeliveryStatus());
                    _rep.SavePacket(iata, previous, current, current_eng, trigger);
                }
            }
            catch (MeteoEmptyException)
            {
                _logger.LogWarning("no metar data {code}", iata);
                return;
            }
            catch (Exception e)
            {
                _logger.LogWarning("{code} Error. {message}", iata, e);
                return;
            }
        }

        private static bool DifferentTimes(MeteoRecord? previous, MeteoRecord current)
        {      
            return previous is null || previous.DateTime < current.DateTime;
        }
    }




}
