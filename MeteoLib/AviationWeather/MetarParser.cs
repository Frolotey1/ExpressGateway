using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MeteoLib.AviationWeather
{
    public static class MetarParser
    {

        public static string GetRawMain(string rawtext)
        {
            return rawtext[..RawNextPos(rawtext, 1)];
        }

        public static string GetRawRmk(string rawtext)
        {
            var pos = rawtext.IndexOf(" RMK");
            if (pos == -1) return "";
            return rawtext[(pos + 1)..];
        }

        public static IEnumerable<MetarTrend> GetTrends(string rawtext)
        {
            var pos = RawNextPos(rawtext, 1);

            while (RawNextPos(rawtext, pos) != rawtext.Length)
            {
                var nextpos = RawNextPos(rawtext, pos + 1);
                var s = rawtext[(pos + 1)..nextpos] + " ";
                var windrow = GetWindRow(s);
                if (s.StartsWith("TEMPO") || s.StartsWith("BECMG"))
                    yield return new MetarTrend
                    {
                        trend_type = s.StartsWith("TEMPO") ? TrendType.TEMPO : TrendType.BECMG,
                        trand_raw = s,
                        time_from = GetTrendTime(s, "FM"),
                        time_to = GetTrendTime(s, "TL"),
                        time_at = GetTrendTime(s, "AT"),
                        visibility_statute_m = GetVisiblity(s),
                        wind_dir_degrees = GetWindDir(windrow),
                        wind_speed = GetWindSpeed(windrow),
                        wind_gust = GetWindGust(windrow),
                        wind_changingdir = GetWindChange(windrow),
                        vertical_visiblity_ft = GetVerticalVisiblity(s),
                        wx_string = GetWX(s),
                        metar_skies = ParseMetarSkies(s).ToList()
                    };
                pos = nextpos;
            }
        }       

        public static double GetAdhesion(string adhrow)
        {
            if (adhrow.Length >= 10)
            {
                if (double.TryParse(adhrow[^2..], out double d)) return d / 100;
                return 0;
            }

            return 0;

        }


        public static string GetAdhesionRow(string adhrow)
        {
            var i = adhrow.IndexOf("/") + 1;
            if (i > 1) return adhrow[i..];
            return "";
        }

        public static string GetAirStrip(string adhrow)
        {
            var i = adhrow.IndexOf("/");
            if (i > 0) return adhrow[1..i];
            return "";
        }



        public static int? GetVisiblity(string rawtext)
        {
            var regex = new Regex(@" (\d{4}) ", RegexOptions.IgnoreCase);
            var match = regex.Matches(rawtext).FirstOrDefault();
            if (match != null) return int.Parse(match.Value.Trim());

            if (rawtext.Contains(" CAVOK ")) return 10000;
            return null;
        }

        public static IEnumerable<MetarAirStrip> ParseMetarStrips(string rawmain)
        {
            //var regex = new Regex(@"R(\d{1,3})([A-Z]?)/(((\w{4})(\d{2}))|(CLRD//)|(//99//)|(SNOCLO)|(//////))", RegexOptions.IgnoreCase);
            var regex = new Regex(@"R(\d{1,2})?[A-Z]?/((\d{1}|/|CLRD)(\d{1}|/)?(\d{2}|//)?(\d{2}|//)|SNOCLO)", RegexOptions.IgnoreCase);
            var matches = regex.Matches(rawmain);
            foreach (Match match in matches)
            {
                yield return new MetarAirStrip()
                {
                    air_strip_code = GetAirStrip(match.Value),
                    adhesion = GetAdhesion(match.Value),
                    adhesionrow = GetAdhesionRow(match.Value)
                };
            }
        }

        public static bool GetChangingWindDir(string rawtext)
        {
            var regex = new Regex(@"VRB(\d{2})MPS", RegexOptions.IgnoreCase);
            var match = regex.Matches(rawtext).FirstOrDefault();
            if (match != null) return true;
            return false;
        }





        public static int? GetAtmPreasure(string rawText)
        {
            Regex regex = new(@" QFE(\d+)/", RegexOptions.IgnoreCase);
            var match = regex.Matches(rawText).FirstOrDefault();
            if (match != null) return int.Parse(match.Value.Replace(" QFE", "").Replace("/", ""));
            return null;
        }


        private static int RawNextPos(string rawtext, int startpos)
        {
            int[] p = {
                rawtext.IndexOf(" TEMPO", startpos),
                rawtext.IndexOf(" BECMG", startpos),
                rawtext.IndexOf(" NOSIG", startpos),
                rawtext.IndexOf(" RMK", startpos),
                rawtext.Length
            };
            return p.Where(i => i != -1).Min();
        }


        public static IEnumerable<MetarSky> ParseMetarSkies(string rawtext)
        {
            var regex = new Regex(@" (((([A-Z]{3})|(//(/?)))((\d{3})|(///))(CB|TCU|///)?)|SKC|CLR|NSC|NCD|OVX|CAVOK) ", RegexOptions.IgnoreCase);
            var s0 = rawtext.Replace(" ", "  ");
            var matches = regex.Matches(s0);
            foreach (Match m in matches)
            {
                var v = m.Value.Trim().Replace("/////", "//////");
                if (v.Length == 3 || v.ToUpper() == "CAVOK")
                    yield return new MetarSky { sky_cover = v };
                else
                {
                    if (int.TryParse(v[3..6], out int agl)) agl *= 100;
                    else agl = -1;
                    yield return new MetarSky { sky_cover = v[0..3], cloud_base_ft_agl = agl, clouds = v[6..].Replace("/", "") };
                }
            }
        }
        public static IEnumerable<MetarSky> ParseTafSkies(string rawtext)
        {
            var regex = new Regex(@"(((([A-Z]{3})|(//(/?)))((\d{3})|(///))(CB|TCU|///)?)|SKC|CLR|NSC|NCD|OVX|CAVOK)", RegexOptions.IgnoreCase);
            var s0 = rawtext.Replace(" ", "  ");
            var matches = regex.Matches(s0);
            foreach (Match m in matches)
            {
                var v = m.Value.Trim().Replace("/////", "//////");
                if (v.Length == 3 || v.ToUpper() == "CAVOK")
                    yield return new MetarSky { sky_cover = v };
                else
                {
                    if (int.TryParse(v[3..6], out int agl)) agl *= 100;
                    else agl = -1;
                    yield return new MetarSky { sky_cover = v[0..3], cloud_base_ft_agl = agl, clouds = v[6..].Replace("/", "") };
                }
            }
        }
        private static string GetWX(string rawtext)
        {
            var s = "";
            var regex = new Regex(@" (\-|\+)?(([A-Z]{2})|([A-Z]{4})|([A-Z]{6})|FLASHES|NSW) ", RegexOptions.IgnoreCase);
            var matches = regex.Matches(rawtext.Replace(" ", "  "));
            foreach (Match m in matches)
                s += m.Value.Trim() + " ";
            return s.Trim();
        }

        private static int? GetVerticalVisiblity(string rawtext)
        {
            var regex = new Regex(@" VV(\d{3}) ", RegexOptions.IgnoreCase);
            var match = regex.Matches(rawtext).FirstOrDefault();
            if (match != null && int.TryParse(match.Value.Trim()[2..5], out var v))
                return v * 100;
            return null;
        }

        private static bool? GetWindChange(string rawtext)
        {
            if (rawtext.Length < 6) return null;
            return rawtext[0..3] == "VRB";
        }

        private static int? GetWindDir(string rawtext)
        {
            if (rawtext.Length < 6) return 0;
            if (int.TryParse(rawtext[0..3], out var winddir))
                return winddir;
            return null;
        }

        private static int? GetWindSpeed(string rawtext)
        {
            if (rawtext.Length < 6) return null;
            var s = rawtext[3..6];
            if (rawtext[3..6] == "P49") return 50;
            if (int.TryParse(rawtext[3..5], out var windspeed))
                return windspeed;
            return null;
        }

        public static string GetRawTemp(string rawtext)
        {
            var regex = new Regex(@" M?(\d{2})/M?(\d{2})", RegexOptions.IgnoreCase);
            var match = regex.Matches(rawtext).FirstOrDefault();
            if (match != null) return match.Value.Trim();
            return string.Empty;
        }

        public static int? GetTemp(string rawtemp)
        {
            if (string.IsNullOrEmpty(rawtemp)) return null;

            var temp_arr = rawtemp.Split('/');
            if (temp_arr.Length != 2) return null;

            if (int.TryParse(temp_arr[0].Replace("M", ""), out var res))
            {
                return res;
            }

            return null;
        }

        public static bool? GetTempSign(string rawtemp)
        {
            if (string.IsNullOrEmpty(rawtemp)) return null;

            var temp_arr = rawtemp.Split('/');
            if (temp_arr.Length != 2) return null;
            return !temp_arr[0].Contains('M');
        }

        public static int? GetDewpoint(string rawtemp)
        {
            if (string.IsNullOrEmpty(rawtemp)) return null;

            var temp_arr = rawtemp.Split('/');
            if (temp_arr.Length != 2) return null;

            if (int.TryParse(temp_arr[1].Replace("M", ""), out var res))
            {
                return res;
            }

            return null;
        }

        public static bool? GetDewpointSign(string rawtemp)
        {
            if (string.IsNullOrEmpty(rawtemp)) return null;

            var temp_arr = rawtemp.Split('/');
            if (temp_arr.Length != 2) return null;
            return !temp_arr[1].Contains('M');
        }



        private static int? GetWindGust(string rawtext)
        {
            var regex = new Regex(@"G(\d{2})", RegexOptions.IgnoreCase);
            var match = regex.Matches(rawtext).FirstOrDefault();
            if (match != null && int.TryParse(match.Value[1..3], out var windgust))
                return windgust;
            return null;
        }

        private static string GetWindRow(string rawtext)
        {
            var regex = new Regex(@" (VRB|(\d{3}))P?(\d{2})(G(\d{2}))?MPS ", RegexOptions.IgnoreCase);

            var match = regex.Matches(rawtext).FirstOrDefault();
            if (match != null)
                return match.Value.Trim();
            return "";
        }

        private static DateTime? GetTrendTime(string rawtext, string timetype)
        {
            var regex = new Regex(" " + timetype + @"(\d{4}) ", RegexOptions.IgnoreCase);
            var match = regex.Matches(rawtext).FirstOrDefault();
            if (match != null)
            {
                var s = match.Value.Trim().Replace(timetype, "");
                s = s.Replace("2400", "0000");
                return DateTime.ParseExact(s, "HHmm", null);
            }
            return null;
        }

        internal static string ParseMountain(string rawrmk)
        {
            var regex = new Regex(@" [A-Z]{2,4} OBSC", RegexOptions.IgnoreCase);
            var match = regex.Matches(rawrmk).FirstOrDefault();
            if (match != null)
            {
                return match.Value.Trim();
            }

            return string.Empty;
        }
    }
}
