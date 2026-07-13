using MeteoLib.AviationWeather;
using Microsoft.Extensions.Primitives;
using System.Text.RegularExpressions;
using static MeteoLib.SourceText;

namespace MeteoLib
{
    public class MeteoRecordFactory
    {
        private readonly SourceText _sourceText;

        public MeteoRecordFactory()
        {
            _sourceText = new SourceText(Language.RUS);
        }

        public MeteoRecordFactory(Language language)
        {
            _sourceText = new SourceText(language);
        }

        public MeteoRecord CreateFrom(Metar m)
        {
            var windspeed = KnotsToMPS(m.wind_speed_kt);
            return new MeteoRecord
            {
                RawText = m.raw_text,
                StationId = m.station_id,
                DateTime = m.observation_time,
                Temperature = GetTemp(m.temp_c, m.temp_c_sing),
                Dewpoint = GetTemp(m.dewpoint_c, m.dewpoint_c_sing),
                TemperatureStr = GetTempStr(m.temp_c, m.temp_c_sing),
                DewpointStr = GetTempStr(m.dewpoint_c, m.dewpoint_c_sing),
                WindSpeed = windspeed,
                WindGust = KnotsToMPS(m.wind_gust_kt),
                WindDirDegrees = m.wind_dir_degrees,
                ChangingWindDir = m.wind_changingdir,
                Conditions = ConditionList(m.wx_string),
                ConditionStings = GetConditionStings(m.wx_string),
                VisiblityValue = m.visibility_statute_m is null ? 0 : (int)m.visibility_statute_m,
                Skies = SkyCoverStrings(m.metar_skies).ToList(),
                AtmPreasure = m.atm_preasure,
                AirStrips = GetAirStrips(m.metar_air_strips).ToList(),
                Visiblity = GetVisiblity(m.visibility_statute_m),
                WindDirection = GetWindDirection(m.wind_dir_degrees, windspeed, m.wind_changingdir),
                MountainVisiblity = GetMountainVisiblity(m.mountain_visiblity),
                Trends = GetTrendStrings(m.trends).ToList()
            };
        }
        public TafMeteoRecord CreateFrom(Taf m)
        {
            var windspeed = m.MainForecast.Weather.Wind.WindSpeed;
            var sky = SkyCoverStrings(m.MainForecast.Skies);
            var taf = new TafMeteoRecord
            {
                MainRaw = m.MainForecast.ForecastRaw,
                RawText = m.RawText,
                MaxTemperature = m.MaxTemperature,
                MinTemperature = m.MinTemperature,
                StationId = m.StationId,
                IssueTime = m.CreatedTime,
                Start = m.MainForecast.Start,
                End = m.MainForecast.End,
                Visibility = GetVisiblity(m.MainForecast.Weather.Visibility),
                MainSkies = sky,
                MainConditions = GetTafConditions(m.MainForecast.Weather.Conditions),
                //Trends = { GetTafTrendString(m.Forecast) },

                Wind = new WindRecord
                {
                    WindDirection = GetWindDirection(m.MainForecast.Weather.Wind.WindDirDegrees, windspeed, m.MainForecast.Weather.Wind.ChangingDir),
                    WindSpeed = windspeed,
                    WindGust = m.MainForecast.Weather.Wind.WindGust
                }
            };

            if (m.ChangingForecasts is not null)
            {
                taf.Trends.AddRange(m.ChangingForecasts.Select(x => GetTafTrendString(x)).ToList());
            }
            return taf;
        }
        private string GetTafConditions(string wxString)
        {
            string conditions = string.Empty;
            if (wxString != "")
                foreach (var wx in wxString.Split(" ").ToList())
                    conditions += (conditions != string.Empty ? ", " : "") + _sourceText.GetConditionString(wx);
            return conditions;
        }
        private string GetInfoString(Forecast forecast)
        {
            string header = BuildInfoHeader(forecast);
            string body = BuildInfoBody(forecast);
            return string.Join(" ", header, body).Trim().ToLower();
        }
        private string BuildInfoBody(Forecast forecast)
        {
            string body = string.Empty;
            if (forecast.Weather.Wind.WindSpeed.HasValue)
            {
                if (forecast.Weather.Wind.WindSpeed == 0)
                    body += _sourceText.WindCalm;
                else
                    body += $"{_sourceText.Wind} {GetWindDirection(forecast.Weather.Wind.WindDirDegrees, forecast.Weather.Wind.WindSpeed, forecast.Weather.Wind.ChangingDir)} {forecast.Weather.Wind.WindSpeed}{_sourceText.WindUnits}";

                if (forecast.Weather.Wind.WindGust.HasValue)
                    body += $" {_sourceText.WindGusts} {forecast.Weather.Wind.WindGust}{_sourceText.WindUnits}";
            }

            if (forecast.Weather.Visibility.HasValue)
                body += (body != "" ? ", " : "") + $"{_sourceText.Visiblity} {GetVisiblity(forecast.Weather.Visibility)}";

            if (forecast.Weather.Conditions != "")
                foreach (var wx in forecast.Weather.Conditions.Split(" ").ToList())
                    body += (body != "" ? ", " : "") + _sourceText.GetConditionString(wx);

            if (forecast.Weather.VerticalVisibility.HasValue)
            {
                if (forecast.Weather.VerticalVisibility == 0)
                    body += (body != "" ? ", " : "") + _sourceText.ClosedSky;
                else
                {
                    body += (body != "" ? ", " : "") + $"{_sourceText.VertVisiblity} {forecast.Weather.VerticalVisibility}{_sourceText.Meters}";
                }
            }
                

            if (forecast.Skies.Any())
            {
                var skies = SkyCoverStrings(forecast.Skies).ToList();
                if (skies.Count > 0)
                    body += (body != "" ? ", " : "") + $"{_sourceText.Cloud} {string.Join(" ", skies)}";
            }
            return body;
        }

        private string BuildInfoHeader(Forecast forecast)
        {
            string header = string.Empty;

            if (forecast.Probability != null)
                header += _sourceText.Probability + $" {forecast.Probability}%";

            if (forecast.Indicator == "TEMPO")
                header += (header != "" ? " " : "") + _sourceText.Tempo;
            return header;
        }

        private TafTrend GetTafTrendString(Forecast forecast)
        {
            return new TafTrend
            {
                TafText = forecast.ForecastRaw,
                Info = GetInfoString(forecast),
                //Info = (ts + s).Trim().ToLower(),
                Probability = forecast.Probability,
                Conditions = GetTafConditions(forecast.Weather.Conditions),
                TimeFrom = forecast.Start,
                TimeTo = forecast.End,
                Skies = SkyCoverStrings(forecast.Skies),
                Wind = new WindRecord
                {
                    WindDirection = GetWindDirection(forecast.Weather.Wind.WindDirDegrees, forecast.Weather.Wind.WindSpeed, forecast.Weather.Wind.ChangingDir),
                    WindSpeed = forecast.Weather.Wind.WindSpeed,
                    WindGust = forecast.Weather.Wind.WindGust
                },
                Visibility = GetVisiblity(forecast.Weather.Visibility),
                VerticalVisibility = forecast.Weather.VerticalVisibility
            };
        }
        private static double? GetTemp(double? temp_c, bool? temp_c_sing)
        {
            if (temp_c == null || temp_c_sing == null) return null;

            if ((bool)temp_c_sing) return temp_c;
            else return -temp_c;
        }

        private static string GetTempStr(double? temp_c, bool? temp_c_sing)
        {
            if (temp_c == null || temp_c_sing == null) return "-";

            if ((bool)temp_c_sing) return temp_c.Value.ToString() + "°";
            else return "-" + temp_c.Value.ToString() + "°";
        }

        private string GetMountainVisiblity(string mountain_visiblity)
        {
            return _sourceText.GetMountainVisiblity(mountain_visiblity);
        }

        private IEnumerable<TrendRecord> GetTrendStrings(List<MetarTrend> trends)
        {
            foreach (var t in trends)
            {
                var s = "";
                var ts = "";
                if (t.trend_type == TrendType.TEMPO) ts += _sourceText.Tempo;

                if (t.wind_speed.HasValue)
                {
                    s += s != "" ? "," : "";
                    if (t.wind_speed == 0) s += " " + _sourceText.WindCalm;
                    else s += $" {_sourceText.Wind} {GetWindDirection(t.wind_dir_degrees, t.wind_speed, t.wind_changingdir)} {t.wind_speed}{_sourceText.WindUnits}";

                    if (t.wind_gust.HasValue) s += $" {_sourceText.WindGusts} {t.wind_gust}{_sourceText.WindUnits}";
                }

                if (t.visibility_statute_m.HasValue) s += (s != "" ? "," : "") + $" {_sourceText.Visiblity} {GetVisiblity(t.visibility_statute_m)}";

                if (t.wx_string != "")
                    foreach (var wx in t.wx_string.Split(" ").ToList())
                        s += (s != "" ? ", " : " ") + _sourceText.GetConditionString(wx);

                var skies = SkyCoverStrings(t.metar_skies).ToList();
                if (skies.Count > 0) s += (s != "" ? ", " : "") + _sourceText.Cloud;
                foreach (var sky in skies)
                    s += " " + sky;

                if (t.vertical_visiblity_ft.HasValue)
                {
                    var v = Math.Round(FootsToMeters((double)t.vertical_visiblity_ft) / 10, 0, MidpointRounding.ToZero) * 10;
                    s += (s != "" ? "," : "") + $" {_sourceText.VertVisiblity} {v}{_sourceText.Meters}";
                }



                yield return new TrendRecord { Info = (ts + s).Trim(), TimeAt = t.time_at, TimeFrom = t.time_from, TimeTo = t.time_to };
            }
        }

        private string GetVisiblity(int? val)
        {
            if (!val.HasValue) return "";
            //if (val.Value == 10000) return _sourceText.Cavok;

            return _sourceText.GetVisiblity(val.Value);
        }

        private string GetWindDirection(int? dir, double? speed, bool? change)
        {
            if (speed != null && speed == 0) return _sourceText.WindCalm;
            if (change != null && change == true) return _sourceText.WindVariable;

            if (dir != null)
            {
                var ind = (int)((Convert.ToDouble(dir) + 22.5) / 45) % 8;
                return _sourceText.WindDirection(ind);
            }
            return "";
        }

        private string GetAdhNote(string adhrow) => _sourceText.GetAdhNote(adhrow);


        public IEnumerable<AirStrip> GetAirStrips(List<MetarAirStrip> metarAirStrips)
        {
            foreach (var air in metarAirStrips)
            {
                yield return new AirStrip()
                {
                    AirStripNo = _sourceText.GetAirStrip(air.air_strip_code),
                    Adhesion = air.adhesion,
                    AdhesionNote = GetAdhNote(air.adhesionrow)
                };
            }
        }

        private IEnumerable<string> SkyCoverStrings(List<MetarSky> skies)
        {
            foreach (var s in skies)
            {
                if (s.sky_cover != "///")
                    yield return _sourceText.GetSkyCoverName(s.sky_cover);

                if (s.clouds == "CB") yield return _sourceText.SkyCB;
                if (s.clouds == "TCU") yield return _sourceText.SkyTCU;

                if ((s.sky_cover == "FEW" || s.sky_cover == "SCT" || s.sky_cover == "BKN" || s.sky_cover == "OVC" || s.sky_cover == "///") && s.cloud_base_ft_agl != -1)
                {
                    var h = Math.Round(FootsToMeters(s.cloud_base_ft_agl) / 10, 0, MidpointRounding.ToZero) * 10;
                    yield return _sourceText.GetSkyLimit(h);
                }

            }
        }


        private static List<string> ConditionList(string wxstring)
        {
            if (string.IsNullOrEmpty(wxstring)) return new();

            return wxstring.Split(" ").ToList();
        }
        private static double? KnotsToMPS(double? knots)
        {
            if (knots == null) return null;
            return Math.Round(knots.Value * 0.514444, 0);
        }
        private static double FootsToMeters(double foots)
        {
            return foots / 3.28084;
        }
        private Dictionary<string, string> GetConditionStings(string wxstring)
        {
            if (string.IsNullOrEmpty(wxstring)) return new();

            var d = new Dictionary<string, string>();
            foreach (var cond in wxstring.Split(" ").ToList())
            {
                d.Add(cond, _sourceText.GetConditionString(cond));
            }
            return d;
        }
    }
}
