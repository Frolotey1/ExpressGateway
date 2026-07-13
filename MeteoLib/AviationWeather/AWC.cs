using MeteoLib.AviationWeather.AwcSource;
using MeteoLib.Factories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.IO;
using System.Security.Authentication;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace MeteoLib.AviationWeather
{
    public class AWC : IAWC
    {

        private readonly ILogger _logger;
        private readonly IAwcSource _source;

        public AWC(IAwcSource source, ILogger<AWC> logger)
        {
            _logger = logger;
            _source = source;
        }


        public Metar? MetarData(string icao)
        {
            ArgumentNullException.ThrowIfNull(icao, nameof(icao));
            var url = _source.GetMetarUrl(icao);

            // var hours = 1;
            // var url = $"https://www.aviationweather.gov/adds/dataserver_current/httpparam?dataSource=metars&requestType=retrieve&format=xml&stationString={icao}&hoursBeforeNow={hours}";
            // var url = $"https://aviationweather.gov/cgi-bin/data/dataserver.php?requestType=retrieve&dataSource=metars&stationString={icao}&hoursBeforeNow={hours}&format=xml";

            var data = LoadMetarData(url);

            _logger.LogInformation($"{DateTime.UtcNow} LoadMetarData {url}");

            return data.FirstOrDefault();
        }

        private async Task<XDocument?> XDocLoad(string url)
        {
            // strongly using ssl, and awoid problems =)
            var hdlr = new HttpClientHandler
            {
                SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls,
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };

            using (var cl = new HttpClient(hdlr))
            {

                try
                {
                    cl.Timeout = TimeSpan.FromSeconds(220);
                    var stream = await cl.GetStreamAsync(url);
                    var doc = XDocument.Load(stream);
                    return doc;
                }
                catch (Exception ex)
                {
                    _logger.LogError("{date} Load Data Error {ex}", DateTime.Now, ex);
                    _logger.LogInformation("source url {URL}", url);
                    return new XDocument();
                }
            }
        }

        private IEnumerable<Metar> LoadMetarData(string url)
        {
            var doc = XDocLoad(url).Result; // XDocument.Load(url);

            try
            {
                var metars = LoadMetarData(doc);

                if (!metars.Any())
                    _logger.LogDebug("no results by url: {URL}", url);

                return metars;
            }
            catch (Exception ex)
            {
                _logger.LogError("Load Data  Error {Exception}", ex);
                _logger.LogInformation("source doc {XML}", doc);
                return new List<Metar>();
            }
        }

        public static IEnumerable<Metar> ParseMetarData(string xml)
        {
            var doc = XDocument.Parse(xml);

            var metars = LoadMetarData(doc);

            return metars;
        }

        private static IEnumerable<Metar> LoadMetarData(XDocument doc)
        {
            var data = doc?.Root?.Element("data");

            if (data is null) yield break;

            var cnt = data.Attribute("num_results");

            if (cnt is null || (int)cnt == 0) yield break;

            foreach (var x in data.Descendants("METAR"))
            {
                var rawtext = AsString(x, "raw_text");
                var rawmain = MetarParser.GetRawMain(rawtext);
                var rawrmk = MetarParser.GetRawRmk(rawtext);
                var rawtemp = MetarParser.GetRawTemp(rawtext);

                yield return new Metar
                {
                    raw_text = rawtext,
                    raw_main = rawmain,
                    raw_rmk = rawrmk,
                    trends = MetarParser.GetTrends(rawtext).ToList(),
                    station_id = AsString(x, "station_id"),
                    observation_time = AsDateTime(x, "observation_time"),
                    //temp_c = AsDouble(x, "temp_c"),
                    //dewpoint_c = AsDouble(x, "dewpoint_c"),
                    temp_c = MetarParser.GetTemp(rawtemp),
                    temp_c_sing = MetarParser.GetTempSign(rawtemp),
                    dewpoint_c = MetarParser.GetDewpoint(rawtemp),
                    dewpoint_c_sing = MetarParser.GetDewpointSign(rawtemp),
                    wind_dir_degrees = AsInteger(x, "wind_dir_degrees"),
                    wind_speed_kt = AsDouble(x, "wind_speed_kt"),
                    wind_gust_kt = AsDouble(x, "wind_gust_kt"),
                    wx_string = AsString(x, "wx_string"),
                    visibility_statute_m = MetarParser.GetVisiblity(rawmain),
                    wind_changingdir = MetarParser.GetChangingWindDir(rawmain),
                    atm_preasure = MetarParser.GetAtmPreasure(rawrmk),
                    metar_skies = MetarParser.ParseMetarSkies(rawmain).ToList(),
                    metar_air_strips = MetarParser.ParseMetarStrips(rawmain).ToList(),
                    mountain_visiblity = MetarParser.ParseMountain(rawrmk)
                };
            }

        }





        private static int? AsInteger(XElement x, XName name)
        {
            var elem = x.Element(name);

            if (elem is null) return null;

            return (int)elem;
        }

        private static double? AsDouble(XElement x, XName name)
        {
            var elem = x.Element(name);

            if (elem is null) return null;

            return (double)elem;
        }


        private static DateTime AsDateTime(XElement x, XName name)
        {
            var val = x.Element(name);

            if (val is null) return default;

            return (DateTime)val;
        }
        private static string AsString(XElement x, XName name)
        {
            var val = x.Element(name);

            if (val is null) return string.Empty;

            return (string)val;
        }

        public Taf? TafData(string icao)
        {
            var url = _source.GetTafUrl(icao);
            var data = LoadTafData(url);

            _logger.LogInformation($"{DateTime.UtcNow} LoadTafData {url}");

            return data.FirstOrDefault();
        }
        private IEnumerable<Taf> LoadTafData(string url)
        {
            var doc = XDocLoad(url).Result; // XDocument.Load(url);  

            try
            {
                var tafs = LoadTafData(doc);

                if (!tafs.Any())
                    _logger.LogInformation("no results by url: {URL}", url);

                return tafs;
            }
            catch (Exception ex)
            {
                _logger.LogError("Load Data  Error {Exception}", ex);
                _logger.LogInformation("source doc {XML}", doc);
                return new List<Taf>();
            }
        }

        private static IEnumerable<Taf> LoadTafData(XDocument doc)
        {
            var data = doc?.Root?.Element("data");

            if (data is null) yield break;

            var cnt = data.Attribute("num_results");

            if (cnt is null || (int)cnt == 0) yield break;

            foreach (var x in data.Descendants("TAF"))
            {
                var rawtext = AsString(x, "raw_text");
                var createdAt = AsDateTime(x, "issue_time");
                yield return new Taf(createdAt, rawtext);
            }
        }

        public static IEnumerable<Taf> ParseTafData(string xml)
        {
            var doc = XDocument.Parse(xml);

            var metars = LoadTafData(doc);

            return metars;
        }


    }
}