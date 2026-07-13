using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteoLib
{
    public class SourceText
    {
        public enum Language { RUS, ENG};

        private readonly Language _language;

        public SourceText(Language language)
        {
            _language = language;
        }


        public string WindVariable => (_language == Language.RUS) ? "переменный" : "variable";
        public string WindCalm => (_language == Language.RUS) ? "штиль" : "wind calm";
        public string WindDirection(int ind)
        {
            if (_language == Language.RUS)
                return ind switch
                {
                    0 => "север",
                    1 => "северо-восток",
                    2 => "восток",
                    3 => "юго-восток",
                    4 => "юг",
                    5 => "юго-запад",
                    6 => "запад",
                    7 => "северо-запад",
                    _ => ind.ToString(),
                };
            else
                return ind switch
                {
                    0 => "north",
                    1 => "northeast",
                    2 => "east",
                    3 => "southeast",
                    4 => "south",
                    5 => "southwest",
                    6 => "west",
                    7 => "northwest",
                    _ => ind.ToString(),
                };

        }


        public string GetMountainVisiblity(string mountain_visiblity)
        {
            if (_language == Language.RUS)
            {
                if (mountain_visiblity == "MT OBSC") return "горы закрыты";
                if (mountain_visiblity == "MAST OBSC") return "мачты закрыты";
                if (mountain_visiblity == "OBST OBSC") return "препятствия закрыты";
                return mountain_visiblity;
            }
            else
            {
                if (mountain_visiblity == "MT OBSC") return "mountains obscured";
                if (mountain_visiblity == "MAST OBSC") return "masts obscured";
                if (mountain_visiblity == "OBST OBSC") return "obstacles obscured";
                return mountain_visiblity;
            }

        }


        public string GetVisiblity(int val)
        {
            if (_language == Language.RUS)
            {
                if (val >= 9999) return "более 10000м";
                return $"{val}м";
            }
            else
            {
                if (val >= 9999) return "more then 10000m";
                return $"{val}m";
            }
        }


        public string GetAdhNote(string adhrow)
        {
            if (_language == Language.RUS)
            {
                if (adhrow == "//////") return "сведений нет (ВПП загрязнена)";
                if (adhrow == "SNOCLO") return "снег на ВПП (АД закрыт)";
                if (adhrow == "//99//") return "проводится очистка ВПП";
                if (adhrow.StartsWith("CLRD")) return "ВПП очищена(ы)";

                return "";
            }
            else
            {
                if (adhrow == "//////") return "no information (runway contaminated)";
                if (adhrow == "SNOCLO") return "snow on the runway (AD closed)";
                if (adhrow == "//99//") return "runway is being cleaned";
                if (adhrow.StartsWith("CLRD")) return "runway cleared";

                return "";
            }
        }


        public string GetAirStrip(string stripcode)
        {
            if (stripcode.Length < 2) return stripcode;

            if (_language == Language.RUS)
            {
                return stripcode[^1] switch
                {
                    'R' => stripcode[0..^1] + " правая",
                    'L' => stripcode[0..^1] + " левая",
                    'C' => stripcode[0..^1] + " центральная",
                    _ => stripcode,
                };
            }
            else
            {
                return stripcode[^1] switch
                {
                    'R' => stripcode[0..^1] + " right",
                    'L' => stripcode[0..^1] + " left",
                    'C' => stripcode[0..^1] + " central",
                    _ => stripcode,
                };
            }
        }



        public string GetSkyCoverName(string code)
        {
            if (_language == Language.RUS)
            {
                if (code == "SKC") return "ясно";
                if (code == "CLR") return "нет облаков ниже 3000м";
                if (code == "CAVOK" || code == "NSC") return "нет значимой для полетов облачности";
                if (code == "FEW") return "незначительная";
                if (code == "SCT") return "разбросанная";
                if (code == "BKN") return "значительная";
                if (code == "OVC") return "сплошная";
                if (code == "NCD") return "нет облачности";
                if (code == "CB") return "кучево-дождевые облака";
                if (code == "TCU") return "мощно-кучевые облака";
                if (code == "OVX") return "состояние неба не определяется";

                return $"Не определено. {code}";
            }
            else
            {
                if (code == "SKC") return "sky is clear";
                if (code == "CLR") return "no significant clouds";
                if (code == "CAVOK" || code == "NSC") return "ceiling and visibility OK";
                if (code == "FEW") return "few";
                if (code == "SCT") return "scattered";
                if (code == "BKN") return "broken";
                if (code == "OVC") return "overcast";
                if (code == "NCD") return "no clouds";
                if (code == "CB") return "cumulonimbus";
                if (code == "TCU") return "towering cumulus";
                if (code == "OVX") return "the state of the sky is not determined";

                return $"Not defined. {code}";
            }
        }

        public string SkyCB => (_language == Language.RUS) ? "кучево-дождевые облака" : "cumulonimbus";

        public string SkyTCU => (_language == Language.RUS) ? "мощно-кучевые облака" : "towering cumulus";


        public string GetSkyLimit(double val) => (_language == Language.RUS) ? $"нижн. гран. {val}м" : $"lower limit {val}m";


        public string GetConditionString(string condition) => (_language == Language.RUS) ? GetConditionStringRus(condition) : GetConditionStringEng(condition);

        private static string GetConditionStringRus(string condition)
        {
            var conditionCode = condition.Replace("+", "").Replace("-", "");

            var near = "";
            if (conditionCode.StartsWith("VC"))
            {
                near = "в окресности аэродрома ";
                conditionCode = conditionCode.Replace("VC", "");
            }

            var intensive = " (умер. интенсивн.)";
            if (condition.Contains('+')) intensive = " (сильн. интенсивн.)";
            if (condition.Contains('-')) intensive = " (слаб. интенсивн.)";

            if (conditionCode == "DZ") return "морось" + intensive;
            if (conditionCode == "RA") return "дождь" + intensive;
            if (conditionCode == "SN") return "снег" + intensive;
            if (EqualConditionCode(conditionCode, "SNRA")) return "снег с дождём" + intensive;
            if (conditionCode == "SG") return "снежные зёрна" + intensive;
            if (conditionCode == "PL") return "ледяная крупа" + intensive;
            if (conditionCode == "SH") return near + "ливень" + intensive;
            if (EqualConditionCode(conditionCode, "SHRA")) return "ливневой дождь" + intensive;
            if (EqualConditionCode(conditionCode, "SHSNRA")) return "ливневой снег с дождем" + intensive;
            if (EqualConditionCode(conditionCode, "SHRAGR")) return "ливневой дождь с градом" + intensive;
            if (EqualConditionCode(conditionCode, "SHSN")) return "ливневой снег" + intensive;
            if (EqualConditionCode(conditionCode, "FZDZ")) return "замерзающая морось" + intensive;
            if (EqualConditionCode(conditionCode, "FZRA")) return "замерзающий дождь" + intensive;
            if (EqualConditionCode(conditionCode, "TSRA")) return "гроза с дождём" + intensive;
            if (EqualConditionCode(conditionCode, "TSRASN")) return "гроза с дождем и снегом" + intensive;
            if (EqualConditionCode(conditionCode, "TSRAGR")) return "гроза с дождём, градом" + intensive;
            if (EqualConditionCode(conditionCode, "TSSN")) return "гроза со снегом" + intensive;
            if (EqualConditionCode(conditionCode, "TSPL")) return "гроза с ледяной крупой" + intensive;
            if (conditionCode == "TS") return near + "гроза без осадков";
            if (EqualConditionCode(conditionCode, "TSGS")) return "гроза со снежной крупой";
            if (EqualConditionCode(conditionCode, "TSGR")) return "гроза с градом";
            if (conditionCode == "GR") return "град";
            if (EqualConditionCode(conditionCode, "SHGR")) return "град";
            if (EqualConditionCode(conditionCode, "SHGS")) return "снежная крупа";
            if (conditionCode == "IC") return "ледяные иглы";
            if (conditionCode == "BR") return "дымка";
            if (conditionCode == "FG") return near + "туман";
            if (EqualConditionCode(conditionCode, "FZFG")) return "замерзающий туман";
            if (EqualConditionCode(conditionCode, "FGIC")) return "ледяной туман";
            if (EqualConditionCode(conditionCode, "MIFG")) return "поземный туман";
            if (EqualConditionCode(conditionCode, "PRFG")) return "туман местами";
            if (EqualConditionCode(conditionCode, "BCFG")) return "туман волнами";
            if (conditionCode == "FU") return "дым";
            if (conditionCode == "VA") return near + "вулканический пепел";
            if (conditionCode == "DU") return "пыль обложная";
            if (EqualConditionCode(conditionCode, "DRDU")) return "пыльный позёмок";
            if (EqualConditionCode(conditionCode, "BLDU")) return near + "пыльная низовая метель";
            if (conditionCode == "SS") return near + "песчаная буря" + intensive;
            if (conditionCode == "DS") return near + "пыльная буря" + intensive;
            if (conditionCode == "HZ") return "мгла";
            if (conditionCode == "FU") return "дым";
            if (conditionCode == "PY") return "водяная пыль";
            if (EqualConditionCode(conditionCode, "DRSN")) return "снежный позёмок";
            if (EqualConditionCode(conditionCode, "BLSN")) return near + "низовая метель";
            if (EqualConditionCode(conditionCode, "BLSA")) return near + "низовая песчаная метель";
            if (conditionCode == "PO") return near + "пыльные вихри";
            if (conditionCode == "SQ") return "шквал";
            if (condition == "FC") return near + "воронкообразное(ые) облако(а)";
            if (condition == "+FC") return near + "торнадо (смерч)";

            if (condition == "NSW") return "прекращение особых явлений погоды";

            return condition;
        }

        private static string GetConditionStringEng(string condition)
        {
            var conditionCode = condition.Replace("+", "").Replace("-", "");

            var near = "";
            if (conditionCode.StartsWith("VC"))
            {
                near = "in the vicinity of the airfield ";
                conditionCode = conditionCode.Replace("VC", "");
            }

            var intensive = " (medium)";
            if (condition.Contains('+')) intensive = " (heavy)";
            if (condition.Contains('-')) intensive = " (light)";

            if (conditionCode == "DZ") return "drizzle" + intensive;
            if (conditionCode == "RA") return "rain" + intensive;
            if (conditionCode == "SN") return "snow" + intensive;
            if (EqualConditionCode(conditionCode, "SNRA")) return "snow rain" + intensive;
            if (conditionCode == "SG") return "snow grains" + intensive;
            if (conditionCode == "PL") return "ice pellets" + intensive;
            if (conditionCode == "SH") return near + "shower" + intensive;
            if (EqualConditionCode(conditionCode, "SHRA")) return "showers rain" + intensive;
            if (EqualConditionCode(conditionCode, "SHSNRA")) return "showers rain with snow" + intensive;
            if (EqualConditionCode(conditionCode, "SHRAGR")) return "showers rain hail" + intensive;
            if (EqualConditionCode(conditionCode, "SHSN")) return "showers snow" + intensive;
            if (EqualConditionCode(conditionCode, "FZDZ")) return "freezing drizzle" + intensive;
            if (EqualConditionCode(conditionCode, "FZRA")) return "freezing rain" + intensive;
            if (EqualConditionCode(conditionCode, "TSRA")) return "thunderstorm with rain" + intensive;
            if (EqualConditionCode(conditionCode, "TSRASN")) return "thunderstorm with rain and snow" + intensive;
            if (EqualConditionCode(conditionCode, "TSRAGR")) return "thunderstorm with rain and hail" + intensive;
            if (EqualConditionCode(conditionCode, "TSSN")) return "thunderstorm with snow" + intensive;
            if (EqualConditionCode(conditionCode, "TSPL")) return "thunderstorm with ice pellets" + intensive;
            if (conditionCode == "TS") return near + "trunderstorm";
            if (EqualConditionCode(conditionCode, "TSGS")) return "thunderstorm with snow pellet";
            if (EqualConditionCode(conditionCode, "TSGR")) return "thunderstorm with hail";
            if (conditionCode == "GR") return "hail";
            if (EqualConditionCode(conditionCode, "SHGR")) return "hail";
            if (EqualConditionCode(conditionCode, "SHGS")) return "snow pellet";
            if (conditionCode == "IC") return "ice crystals";
            if (conditionCode == "BR") return "mist";
            if (conditionCode == "FG") return near + "fog";
            if (EqualConditionCode(conditionCode, "FZFG")) return "freezing fog";
            if (EqualConditionCode(conditionCode, "FGIC")) return "ice crystals fog";
            if (EqualConditionCode(conditionCode, "MIFG")) return "shallow fog";
            if (EqualConditionCode(conditionCode, "PRFG")) return "partial fog";
            if (EqualConditionCode(conditionCode, "BCFG")) return "patches fog";
            if (conditionCode == "FU") return "fume/smoke";
            if (conditionCode == "VA") return near + "volcanic ash";
            if (conditionCode == "DU") return "widespread dust";
            if (EqualConditionCode(conditionCode, "DRDU")) return "dusty snow";
            if (EqualConditionCode(conditionCode, "BLDU")) return near + "dusty blowing blizzard";
            if (conditionCode == "SS") return near + "sand storm" + intensive;
            if (conditionCode == "DS") return near + "dust storm" + intensive;
            if (conditionCode == "HZ") return "haze";
            if (conditionCode == "PY") return "spray";
            if (EqualConditionCode(conditionCode, "DRSN")) return "low drifting snow";
            if (EqualConditionCode(conditionCode, "BLSN")) return near + "blowing snow";
            if (EqualConditionCode(conditionCode, "BLSA")) return near + "blowing sand";
            if (conditionCode == "PO") return near + "dust/sand whirls";
            if (conditionCode == "SQ") return "squalls";
            if (condition == "FC") return near + "funnel clouds";
            if (condition == "+FC") return near + "tornado or water spout";

            if (condition == "NSW") return "cessation of significant weather";

            return condition;
        }


        public static bool EqualConditionCode(string a, string b)
        {
            if (a == b) return true;

            if (a.Length != b.Length || a.Length % 2 == 1) return false;

            var arr = new List<string>();

            int i = 0;
            while (i < a.Length)
            {
                arr.Add(a[i..(i + 2)]);
                i += 2;
            }

            foreach (var item in arr)
            {
                if (a.LastIndexOf(item) != a.IndexOf(item))
                    return false; //строка a содержит неуникальные пары 

                if (!b.Contains(item))
                    return false; //строка b не содержит пару из a
            }

            return true;
        }




        public string Wind => (_language == Language.RUS) ? "ветер" : "wind";
        public string WindUnits => (_language == Language.RUS) ? "м/с" : "m/s";
        public string WindGusts => (_language == Language.RUS) ? "с порывами" : "gusts";
        public string Visiblity => (_language == Language.RUS) ? "видимость" : "visibility";
        public string Probability => (_language == Language.RUS) ? "С вероятностью" : "With probability"; 
        public string Tempo => (_language == Language.RUS) ? "Временами" : "Temporary";
        public string VertVisiblity => (_language == Language.RUS) ? "вертикальная видимость" : "vertical visibility";

        public string Meters => (_language == Language.RUS) ? "м" : "m";

        public string Cloud => (_language == Language.RUS) ? "облачность" : "cloud";

        public string ClosedSky => (_language == Language.RUS) ? "небо закрыто" : "closed sky";

        public string Cavok => (_language == Language.RUS) ? "нет значимой для полетов облачности" : "ceiling and visibility OK";
    }
}
