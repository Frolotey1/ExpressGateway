using MeteoLib;
using MeteoLib.AviationWeather;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace xUnitTests
{
    public class MeteoRecordTests
    {

        [Theory]
        [MemberData(nameof(WindDirectionData))]
        public void WindDirection_0(List<int> degrees, string direct)
        {
            foreach (var d in degrees)
            {
                var m = new MeteoRecordFactory().CreateFrom(new Metar { wind_dir_degrees = d, wind_speed_kt = 1 });
                Assert.Equal(direct, m.WindDirection);
            }
        }

        public static IEnumerable<object[]> WindDirectionData()
        {
            yield return new object[] { new List<int> { 338, 360, 0, 22 }, "север" };
            yield return new object[] { new List<int> { 23, 45, 67 }, "северо-восток" };
            yield return new object[] { new List<int> { 68, 90, 112 }, "восток" };
            yield return new object[] { new List<int> { 113, 135, 157 }, "юго-восток" };
            yield return new object[] { new List<int> { 158, 180, 202 }, "юг" };
            yield return new object[] { new List<int> { 203, 225, 247 }, "юго-запад" };
            yield return new object[] { new List<int> { 248, 270, 292 }, "запад" };
            yield return new object[] { new List<int> { 293, 315, 337 }, "северо-запад" };

        }

        [Theory]
        [InlineData("", "")]
        [InlineData("MT OBSC", "горы закрыты")]
        [InlineData("MAST OBSC", "мачты закрыты")]
        [InlineData("OBST OBSC", "препятствия закрыты")]
        public void Mountain_0(string mountain, string m_text)
        {
            var m = new MeteoRecordFactory().CreateFrom(new Metar { mountain_visiblity = mountain });

            Assert.Equal(m_text, m.MountainVisiblity);
        }


        [Fact]
        public void Wind_0()
        {
            var m = new MeteoRecordFactory().CreateFrom(new Metar { wind_changingdir = true, wind_speed_kt = 1 });
            Assert.Equal("переменный", m.WindDirection);
        }

        [Fact]
        public void Wind_1()
        {
            var m = new MeteoRecordFactory().CreateFrom(new Metar { wind_changingdir = true, wind_speed_kt = 0 });
            Assert.Equal("штиль", m.WindDirection);
        }

        [Fact]
        public void Wind_2()
        {
            var m = new MeteoRecordFactory().CreateFrom(new Metar { wind_dir_degrees = 90, wind_changingdir = false, wind_speed_kt = 0 });
            Assert.Equal("штиль", m.WindDirection);
        }

        [Theory]
        [InlineData(97, "97м")]
        [InlineData(158, "158м")]
        [InlineData(5433, "5433м")]
        [InlineData(5438, "5438м")]
        [InlineData(9900, "9900м")]
        [InlineData(9999, "более 10000м")]
        [InlineData(10000, "более 10000м")]
        public void Visiblity_0(int val, string str)
        {
            var m = new MeteoRecordFactory().CreateFrom(new Metar { visibility_statute_m = val });
            Assert.Equal(str, m.Visiblity);
        }


        [Fact]
        public void SkyCoverCombo_0()
        {
            var sc = new List<MetarSky>
            {
                new MetarSky { clouds = "CB", cloud_base_ft_agl = 5541, sky_cover = "SCT" },   //1688,9
                new MetarSky { clouds = "TCU", cloud_base_ft_agl = 10608, sky_cover = "BKN" }, //3233,3
                new MetarSky { clouds = "", cloud_base_ft_agl = 14524, sky_cover = "OVC" }    //4426,9
            };
            var m = new MeteoRecordFactory().CreateFrom(new Metar { metar_skies = sc });
            Assert.Equal(8, m.Skies.Count);
            Assert.Equal("разбросанная", m.Skies[0]);
            Assert.Equal("кучево-дождевые облака", m.Skies[1]);
            Assert.Equal("нижн. гран. 1680м", m.Skies[2]);
            Assert.Equal("значительная", m.Skies[3]);
            Assert.Equal("мощно-кучевые облака", m.Skies[4]);
            Assert.Equal("нижн. гран. 3230м", m.Skies[5]);
            Assert.Equal("сплошная", m.Skies[6]);
            Assert.Equal("нижн. гран. 4420м", m.Skies[7]);
        }

        [Theory]
        [InlineData("SCT", "разбросанная", 2)]
        [InlineData("BKN", "значительная", 2)]
        [InlineData("OVC", "сплошная", 2)]
        [InlineData("FEW", "незначительная", 2)]
        [InlineData("NSC", "нет значимой для полетов облачности", 1)]
        [InlineData("NCD", "нет облачности", 1)]
        public void SkyCover_0(string code, string name, int cnt)
        {
            var sc = new List<MetarSky>
            {
                new MetarSky { clouds = "", cloud_base_ft_agl = 5541, sky_cover = code }
            };
            var m = new MeteoRecordFactory().CreateFrom(new Metar { metar_skies = sc });
            Assert.Equal(cnt, m.Skies.Count);
            Assert.Equal(name, m.Skies[0]);
        }

        [Theory]
        [InlineData("FEW", -1, "", "незначительная; ")]
        [InlineData("SCT", -1, "", "разбросанная; ")]
        [InlineData("BKN", -1, "", "значительная; ")]
        [InlineData("OVC", -1, "", "сплошная; ")]
        [InlineData("///", -1, "CB", "кучево-дождевые облака; ")]
        [InlineData("///", -1, "TCU", "мощно-кучевые облака; ")]
        [InlineData("BKN", 2500, "", "значительная; нижн. гран. 760м; ")]
        [InlineData("///", 1500, "", "нижн. гран. 450м; ")]
        [InlineData("///", -1, "", "")]
        public void SkyCover_1(string cover, int agl, string clouds, string sky)
        {
            var sc = new List<MetarSky>
            {
                new MetarSky { clouds = clouds, cloud_base_ft_agl = agl, sky_cover = cover },
            };
            var m = new MeteoRecordFactory().CreateFrom(new Metar { metar_skies = sc });
            //Assert.Equal(8, m.Skies.Count);
            string res = "";
            foreach (var s in m.Skies)
                res += s + "; ";
            Assert.Equal(sky, res);
        }




        [Theory]
        [InlineData("45R", "//////", "45 правая", "сведений нет (ВПП загрязнена)")]
        [InlineData("88L", "SNOCLO", "88 левая", "снег на ВПП (АД закрыт)")]
        [InlineData("7C", "//99//", "7 центральная", "проводится очистка ВПП")]
        [InlineData("08", "CLRD60", "08", "ВПП очищена(ы)")]
        [InlineData("55", "155475", "55", "")]
        public void AdhesionRow_0(string aircode, string adhcode, string airname, string adhname)
        {
            var airs = new List<MetarAirStrip>
            {
                new MetarAirStrip { air_strip_code = aircode, adhesionrow = adhcode }
            };
            var m = new MeteoRecordFactory().CreateFrom(new Metar { metar_air_strips = airs });
            Assert.Single(m.AirStrips);
            var air = m.AirStrips.FirstOrDefault();
            Assert.Equal(adhname, air.AdhesionNote);
            Assert.Equal(airname, air.AirStripNo);
        }

        [Theory]
        [InlineData("45R", "//////", "45 правая", "сведений нет (ВПП загрязнена)", "88L", "SNOCLO", "88 левая", "снег на ВПП (АД закрыт)")]
        [InlineData("7C", "//99//", "7 центральная", "проводится очистка ВПП", "08", "CLRD60", "08", "ВПП очищена(ы)")]
        public void AdhesionRow_1(string aircode, string adhcode, string airname, string adhname, string aircode2, string adhcode2, string airname2, string adhname2)
        {
            var airs = new List<MetarAirStrip>
            {
                new MetarAirStrip { air_strip_code = aircode, adhesionrow = adhcode },
                new MetarAirStrip { air_strip_code = aircode2, adhesionrow = adhcode2 }
            };
            var m = new MeteoRecordFactory().CreateFrom(new Metar { metar_air_strips = airs });
            Assert.Equal(2, m.AirStrips.Count);
            var air = m.AirStrips.FirstOrDefault();
            Assert.Equal(adhname, air.AdhesionNote);
            Assert.Equal(airname, air.AirStripNo);
            var air2 = m.AirStrips.LastOrDefault();
            Assert.Equal(adhname2, air2.AdhesionNote);
            Assert.Equal(airname2, air2.AirStripNo);
        }

        [Theory]
        [InlineData("SHRA", "ливневой дождь (умер. интенсивн.)")]
        [InlineData("+SHRA", "ливневой дождь (сильн. интенсивн.)")]
        [InlineData("-SHRA", "ливневой дождь (слаб. интенсивн.)")]
        [InlineData("SH", "ливень (умер. интенсивн.)")]
        [InlineData("VCSH", "в окресности аэродрома ливень (умер. интенсивн.)")]
        [InlineData("-VCSH", "в окресности аэродрома ливень (слаб. интенсивн.)")]
        [InlineData("+VCSH", "в окресности аэродрома ливень (сильн. интенсивн.)")]
        [InlineData("+SH", "ливень (сильн. интенсивн.)")]
        [InlineData("FU", "дым")]
        [InlineData("MIFG", "поземный туман")]
        [InlineData("DS", "пыльная буря (умер. интенсивн.)")]
        [InlineData("VCDS", "в окресности аэродрома пыльная буря (умер. интенсивн.)")]
        public void Conditions_0(string code, string name)
        {
            var m = new MeteoRecordFactory().CreateFrom(new Metar { wx_string = code });
            Assert.Single(m.ConditionStings);
            Assert.Equal(name, m.ConditionStings[code]);
        }

        [Theory]
        [InlineData("SHRA", "RASH", true)]
        [InlineData("RA", "RA", true)]
        [InlineData("RA", "SA", false)]
        [InlineData("SH", "RASH", false)]
        [InlineData("SHR", "RASH", false)]
        [InlineData("SHRA", "RAS", false)]
        [InlineData("SHRAGR", "SHRAGR", true)]
        [InlineData("SHRAGR", "SHGRRA", true)]
        [InlineData("SHRAGR", "RASHGR", true)]
        [InlineData("SHRAGR", "GRSHRA", true)]
        [InlineData("SHRAGR", "RAGRSH", true)]
        [InlineData("SHRAGR", "GRRASH", true)]
        public void ConditionEqual0(string a, string b, bool res)
        {
            Assert.Equal(res, SourceText.EqualConditionCode(a, b));
        }


        [Fact]
        public void Trends_0()
        {
            var s = new List<MetarSky>
            {
                new MetarSky { clouds = "CB", sky_cover = "OVC", cloud_base_ft_agl = 1600 }
            };
            var t = new MetarTrend { trend_type = TrendType.BECMG, time_at = DateTime.ParseExact("11:00", "HH:mm", null), wx_string = "SH", metar_skies = s, visibility_statute_m = 5000 };
            var m = new MeteoRecordFactory().CreateFrom(new Metar { trends = { t } });
            Assert.Single(m.Trends);
            Assert.Equal("видимость 5000м, ливень (умер. интенсивн.), облачность сплошная кучево-дождевые облака нижн. гран. 480м", m.Trends.First().Info);
        }

        [Fact]
        public void Trends_1()
        {
            var t = new MetarTrend { trend_type = TrendType.BECMG, time_from = DateTime.ParseExact("11:00", "HH:mm", null), time_to = DateTime.ParseExact("12:00", "HH:mm", null), wx_string = "FU", vertical_visiblity_ft = 500 };
            var m = new MeteoRecordFactory().CreateFrom(new Metar { trends = { t } });
            Assert.Single(m.Trends);
            Assert.Equal("дым, вертикальная видимость 150м", m.Trends.First().Info);
        }

        [Fact]
        public void Trends_2()
        {
            var t = new MetarTrend { trend_type = TrendType.TEMPO, wind_changingdir = true, wind_gust = 20, wind_speed = 10 };
            var m = new MeteoRecordFactory().CreateFrom(new Metar { trends = { t } });
            Assert.Single(m.Trends);
            Assert.Equal("Временами ветер переменный 10м/с с порывами 20м/с", m.Trends.First().Info);
        }

        [Fact]
        public void Trends_3()
        {
            var t = new MetarTrend { trend_type = TrendType.BECMG, time_at = DateTime.ParseExact("11:00", "HH:mm", null), wx_string = "NSW" };
            var m = new MeteoRecordFactory().CreateFrom(new Metar { trends = { t } });
            Assert.Single(m.Trends);
            Assert.Equal("прекращение особых явлений погоды", m.Trends.First().Info);
        }

        [Fact]
        public void Trends_4()
        {
            var t = new MetarTrend { trend_type = TrendType.BECMG, time_from = DateTime.ParseExact("11:00", "HH:mm", null), time_to = DateTime.ParseExact("12:00", "HH:mm", null), vertical_visiblity_ft = 300 };
            var m = new MeteoRecordFactory().CreateFrom(new Metar { trends = { t } });
            Assert.Single(m.Trends);
            Assert.Equal("вертикальная видимость 90м", m.Trends.First().Info);
        }

        [Theory]
        [InlineData("11:00", "12:00", null)]
        [InlineData("23:59", "00:00", null)]
        [InlineData(null, null, "11:00")]
        [InlineData(null, null, "23:59")]
        public void TrendsTime_0(string from, string to, string at)
        {
            DateTime? timefrom = (from == null) ? null : DateTime.ParseExact(from, "HH:mm", null);
            DateTime? timeto = (to == null) ? null : DateTime.ParseExact(to, "HH:mm", null);
            DateTime? timeat = (at == null) ? null : DateTime.ParseExact(at, "HH:mm", null);
            var t = new MetarTrend { trend_type = TrendType.BECMG, time_from = timefrom, time_to = timeto, time_at = timeat };
            var m = new MeteoRecordFactory().CreateFrom(new Metar { trends = { t } });
            Assert.Single(m.Trends);
            Assert.Equal(timefrom, m.Trends.First().TimeFrom);
            Assert.Equal(timeto, m.Trends.First().TimeTo);
            Assert.Equal(timeat, m.Trends.First().TimeAt);
        }


    }
}
