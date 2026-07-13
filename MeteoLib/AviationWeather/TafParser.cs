using MeteoLib.Factories;
using System.Text.RegularExpressions;

namespace MeteoLib.AviationWeather
{
    public static class TafParser
    {
        private static readonly string[] _conditions = new string[]
        {
                "DZ","RA","SN","SG","PL","SH", "GR",
                "FZ", "TS","GS", "IC", "BR", "FG", "MI", "PR", "BC",
                "FU", "VA","DU","DR","BL","DS","HZ","PY","DR","SA","PO","SQ","FC",
                "NSW","SS", "GR"
            };
        public static IEnumerable<DateTime> GetTafTimeLimits(string row, DateTime utc)
        {
            var regex = new Regex(@"(\d{4}/\d{4})|(FM\d{2}\d{2}\d{2})", RegexOptions.IgnoreCase);
            var match = regex.Matches(row).FirstOrDefault();
            if (match == null)
            {
                throw new Exception("Not found datetime");
            }
            if (match.Groups[0].Value.StartsWith("FM"))
                yield return AsFmDateTime(match.Groups[0].Value, utc);
            else
            {
                foreach(var item in match.Groups[0].Value.Split('/'))
                    yield return AsDateTime(item, utc);
            }
        }
        private static DateTime AsDateTime(string text, DateTime utc)
        {
            int day = int.Parse(text[..2]);
            int hours = int.Parse(text[2..]);

            return DateCalculate.DecideProbableDate(day, hours, 0, utc);
        }
        public static DateTime AsFmDateTime(string text, DateTime utc)
        {
            int day = int.Parse(text[2..4]);
            int hours = int.Parse(text[4..6]);
            int minutes = int.Parse(text[6..]);

            return DateCalculate.DecideProbableDate(day, hours, minutes, utc);
        }
        public static List<string> GetRows(string text)
        {
            var result = new List<string>();
            var pos = RawNextPos(text, 1);
            //string previousLine = text[..pos];
            result.Add(text[..pos]);
            while (pos < text.Length)
            {
                var nextpos = RawNextPos(text, pos + 1);
                var nextLine = text[(pos + 1)..nextpos];
                result.Add(nextLine);
                pos = nextpos;
            }
            result = ConcatRows(result).ToList();
            return result;
        }
        public static int? GetVisiblity(string rawtext)
        {
            var regex = new Regex(@"(\s\d{4}\s)|(\s\d{4}$)|CAVOK", RegexOptions.IgnoreCase);
            var match = regex.Matches(rawtext).FirstOrDefault();
            if (match != null)
            {
                if (match.Value == "CAVOK")
                    return 10000;
                else
                    return int.Parse(match.Value.Trim());
            }
            return null;
        }
        private static int RawNextPos(string rawtext, int startpos)
        {
            int[] p = {
                rawtext.IndexOf(" TEMPO", startpos),
                rawtext.IndexOf(" BECMG", startpos),
                rawtext.IndexOf(" FM", startpos),
                rawtext.IndexOf(" PROB", startpos),
                rawtext.Length
            };
            return p.Where(i => i != -1).Min();
        }
        public static string GetRawTemp(string rawtext)
        {
            var regex = new Regex(@"((TX)|(TXM))(\d{2})/((\d{4}\w)|(\d{4})) ((TN)|(TNM))(\d{2})/((\d{4}\w)|(\d{4}))", RegexOptions.IgnoreCase);
            var match = regex.Matches(rawtext).FirstOrDefault();
            if (match != null) return match.Value.Trim();
            return string.Empty;
        }        
        public static double? KnotsToMPS(double? knots)
        {
            if (knots == null) return null;
            return Math.Round(knots.Value * 0.514444, 0);
        }
        public static Temperature? ParseTemperature(string temptext, DateTime CreateDate)
        {
            string[] data = temptext.Split('/');
            if (data.Length != 2)
                return null;

            Temperature t = new();
            if (data[0].Contains('M'))
                t.UnderZero = true;

            int day = int.Parse(data[1][..2]);
            int hour = int.Parse(data[1][2..4]);

            t.DateTime = DateCalculate.DecideProbableDate(day, hour, 0, CreateDate);

            t.TemperatureValue = int.Parse(data[0][^2..]);
            return t;
        }
        private static IEnumerable<string> ConcatRows(List<string> rows)
        {
            string prob = string.Empty;
            foreach (var row in rows)
            {
                if (row.Contains("PROB"))
                {
                    if (row.Length > 6)
                        yield return row;
                    else
                        prob = row;
                }
                else
                {
                    if (!string.IsNullOrEmpty(prob))
                    {
                        yield return $"{prob} {row}";
                        prob = string.Empty;
                    }
                    else
                        yield return row;
                }
            }
        }
        internal static string GetCondtions(string rawtext)
        {
            var parts = rawtext.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string[] conditions = parts.Where(x => IsConditionCode(x)).ToArray();
            return string.Join(" ", conditions);
        }
        private static bool IsConditionCode(string text)
        {
            text = text.Replace("+", "").Replace("-", "");

            if (text == "NSW")
                return true;

            if (_conditions.Any(x => text.Contains(x)) && text.Length % 2 == 0)
            {
                var pare = ConditionSplit(text);
                return pare.All(x => _conditions.Contains(x));
            }
            return false;
        }

        private static List<string> ConditionSplit(string x)
        {
            List<string> conditions = new();
            for (int i = 0; i < x.Length; i += 2)
            {
                conditions.Add(x.Substring(i, 2));
            }
            return conditions;
        }

        internal static Wind GetWind(string row)
        {
            var regex = new Regex(@"(\d{3}|VRB)(\d{2})?(G\d{2}|Р\d{2})?(MPS|KT)", RegexOptions.IgnoreCase);
            //var regex = new Regex(@"(\d{3})(\d{2})(G\d{2})?(MPS|KT)", RegexOptions.IgnoreCase);
            var match = regex.Matches(row).FirstOrDefault();
            if (match != null)
            {
                bool IsMPS = match.Groups[4].Value == "MPS";

                if (match.Groups[1].Value == "VRB")//VRB wind
                {
                    double? speedVRB = Convert.ToDouble(match.Groups[2].Value);
                    return new Wind()
                    {
                        ChangingDir = true,
                        WindSpeed = IsMPS ? speedVRB : KnotsToMPS(speedVRB)
                    };
                }

                if (match.Groups[3].Value != string.Empty && match.Groups[3].Value[..1] == "Р")//wind speed more than 50 m\s
                {
                    double? speedP = Convert.ToDouble(match.Groups[3].Value[1..]);
                    return new Wind()
                    {
                        WindDirDegrees = Convert.ToInt32(match.Groups[1].Value),
                        WindSpeed = IsMPS ? speedP : KnotsToMPS(speedP)
                    };
                }

                double? speed = Convert.ToDouble(match.Groups[2].Value);//default wind parse
                Wind wind = new()
                {
                    WindDirDegrees = Convert.ToInt32(match.Groups[1].Value),
                    WindSpeed = IsMPS ? speed : KnotsToMPS(speed)
                };
                if (match.Groups[3].Value != string.Empty)
                {
                    double? gust = Convert.ToDouble(match.Groups[3].Value[1..]);
                    wind.WindGust = IsMPS ? gust : KnotsToMPS(gust);
                }
                return wind;
            }
            return new Wind();
        }

        internal static double? GetVerticalVisibility(string row)
        {
            var regex = new Regex(@"(VV(\d{3}|///))", RegexOptions.IgnoreCase);
            var match = regex.Matches(row).FirstOrDefault();
            if (match == null)
            {
                return null;
            }
            if (match.Groups[2].Value == "///")
                return 0;
            else
                return Convert.ToDouble(match.Groups[2].Value)*30;
        }
        
        internal static Weather GetWeather(string row)
        {
            return new Weather
            {
                Visibility = GetVisiblity(row),
                VerticalVisibility = GetVerticalVisibility(row),
                Conditions = GetCondtions(row),
                Wind = GetWind(row)
            };
        }
        public static (Temperature? max, Temperature? min) GetTemperature(string rawtext, DateTime date)
        {
            var rawtemp = GetRawTemp(rawtext);
            var temperaturesText = rawtemp.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (!temperaturesText.Any() || temperaturesText.Length < 2)
                return (null, null);

            return (ParseTemperature(temperaturesText[0], date), ParseTemperature(temperaturesText[1], date));
        }
    }
}
