using MeteoLib.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MeteoLib.AviationWeather
{
    public class Taf
    {
        readonly DateTime _now = DateTime.UtcNow;
        readonly string _rawtext;
        public Taf(DateTime now, string rawtext)
        {
            this._now = now;
            this._rawtext = rawtext;
            Init();
        }
        private void Init()
        {
            var rows = TafParser.GetRows(_rawtext);
            MainForecast = new Forecast(_now, rows[0]);            
            var (max, min) = TafParser.GetTemperature(_rawtext, CreatedTime);
            MaxTemperature = max;
            MinTemperature = min;
            for (int i = 1; i < rows.Count; i++)
            {
                ChangingForecasts.Add(new Forecast(_now, rows[i]));
            }
        }

        public string StationId
        {
            get
            {
                var regex = new Regex(@"TAF (AMD|COR)?\s?([A-Z]{4})", RegexOptions.IgnoreCase);
                var match = regex.Matches(_rawtext).FirstOrDefault();
                if (match != null)
                    return match.Groups[^1].Value.Trim();
                throw new Exception("Not found station" + _rawtext);
            }
        }
        public string RawText
        {
            get
            {
                return _rawtext;
            }
        }
        public DateTime CreatedTime
        {
            get
            {
                return _now;
            }
        }
        public Temperature? MinTemperature { get; private set; }
        public Temperature? MaxTemperature { get; private set; }
        public Forecast MainForecast { get; private set; }
        public List<Forecast>? ChangingForecasts { get; private set; } = new();

    }
    public class Temperature
    {
        public bool UnderZero { get; set; }
        public int TemperatureValue { get; set; }
        public DateTime DateTime { get; set; }
    }
    public class Forecast
    {
        readonly DateTime _now = DateTime.UtcNow;
        public Forecast(DateTime now, string rawtext)
        {
            this._now = now;
            this.ForecastRaw = rawtext;
            Init();
        }

        private void Init()
        {
            var timeLimit = TafParser.GetTafTimeLimits(ForecastRaw, _now).ToArray();
            switch (Indicator)
            {
                case "TEMPO":
                    Start = timeLimit[0];
                    End = timeLimit[1];
                    break;
                case "FM":
                    Start = timeLimit[0];
                    break;
                case "BECMG":
                    Start = timeLimit[0];
                    End = timeLimit[1];
                    break;
                case "PROB":
                    Start = timeLimit[0];
                    End = timeLimit[1];
                    break;
                case "TAF":
                    Start = timeLimit[0];
                    End = timeLimit[1];
                    break;
                default:
                    throw new NotImplementedException(Indicator);
            }
        }

        public string ForecastRaw { get; private set; }
        public string Indicator 
        {
            get
            {
                var regex = new Regex(@"TAF|BECMG|FM|PROB\d{2}|TEMPO", RegexOptions.IgnoreCase);
                var match = regex.Matches(ForecastRaw);
                if (match != null)
                {
                    return Regex.Replace(match[^1].Groups[0].Value, @"[\d-]", string.Empty);
                }
                throw new Exception("No indicator in raw: " + ForecastRaw);
            }
        }
        public DateTime Start { get; private set; }
        public DateTime? End { get; private set; }
        public Weather Weather {
            get
            {
                return TafParser.GetWeather(ForecastRaw);
            }
        }
        public int? Probability { 
            get 
            {
                var regex = new Regex(@"(PROB)(\d{2})", RegexOptions.IgnoreCase);
                var match = regex.Matches(ForecastRaw).FirstOrDefault();
                if (match != null)
                    return int.Parse(match.Value[^2..].Trim());
                return null;
            } 
        }
        public List<MetarSky> Skies {
            get
            {
                return MetarParser.ParseTafSkies(ForecastRaw).ToList();
            }
        }
    }
    public class Weather
    {
        public Wind Wind { get; set; }
        public int? Visibility { get; set; }
        public double? VerticalVisibility { get; set; }
        public string Conditions { get; set; } = string.Empty;

        //public IEnumerable<string> Skies { get; set; }
    }
    public class Wind
    {
        //public string WindDirection { get; set; } = string.Empty;
        public bool ChangingDir { get; set; }
        public double? WindSpeed { get; set; }
        public double? WindGust { get; set; }
        public int? WindDirDegrees { get; set; }
    }
}
