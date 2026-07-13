using MeteoLib;
using MeteoLib.AviationWeather;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static MeteoLib.SourceText;

namespace xUnitTests
{
    public class MeteoRecordEngTests
    {

        [Theory]
        [MemberData(nameof(WindDirectionData))]
        public void WindDirection_0(List<int> degrees, string direct)
        {
            foreach (var d in degrees)
            {
                var m = new MeteoRecordFactory(Language.ENG).CreateFrom(new Metar { wind_dir_degrees = d, wind_speed_kt = 1 });
                Assert.Equal(direct, m.WindDirection);
            }
        }

        public static IEnumerable<object[]> WindDirectionData()
        {
            yield return new object[] { new List<int> { 338, 360, 0, 22 }, "north" };
            yield return new object[] { new List<int> { 23, 45, 67 }, "northeast" };
            yield return new object[] { new List<int> { 68, 90, 112 }, "east" };
            yield return new object[] { new List<int> { 113, 135, 157 }, "southeast" };
            yield return new object[] { new List<int> { 158, 180, 202 }, "south" };
            yield return new object[] { new List<int> { 203, 225, 247 }, "southwest" };
            yield return new object[] { new List<int> { 248, 270, 292 }, "west" };
            yield return new object[] { new List<int> { 293, 315, 337 }, "northwest" };
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("MT OBSC", "mountains obscured")]
        [InlineData("MAST OBSC", "masts obscured")]
        [InlineData("OBST OBSC", "obstacles obscured")]
        public void Mountain_0(string mountain, string m_text)
        {
            var m = new MeteoRecordFactory(Language.ENG).CreateFrom(new Metar { mountain_visiblity = mountain });

            Assert.Equal(m_text, m.MountainVisiblity);
        }


        [Fact]
        public void Wind_0()
        {
            var m = new MeteoRecordFactory(Language.ENG).CreateFrom(new Metar { wind_changingdir = true, wind_speed_kt = 1 });
            Assert.Equal("variable", m.WindDirection);
        }

        [Fact]
        public void Wind_1()
        {
            var m = new MeteoRecordFactory(Language.ENG).CreateFrom(new Metar { wind_changingdir = true, wind_speed_kt = 0 });
            Assert.Equal("wind calm", m.WindDirection);
        }

        [Fact]
        public void Wind_2()
        {
            var m = new MeteoRecordFactory(Language.ENG).CreateFrom(new Metar { wind_dir_degrees = 90, wind_changingdir = false, wind_speed_kt = 0 });
            Assert.Equal("wind calm", m.WindDirection);
        }

        [Theory]
        [InlineData(97, "97m")]
        [InlineData(158, "158m")]
        [InlineData(5433, "5433m")]
        [InlineData(5438, "5438m")]
        [InlineData(9900, "9900m")]
        [InlineData(9999, "more then 10000m")]
        [InlineData(10000, "more then 10000m")]
        public void Visiblity_0(int val, string str)
        {
            var m = new MeteoRecordFactory(Language.ENG).CreateFrom(new Metar { visibility_statute_m = val });
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
            var m = new MeteoRecordFactory(Language.ENG).CreateFrom(new Metar { metar_skies = sc });
            Assert.Equal(8, m.Skies.Count);
            Assert.Equal("scattered", m.Skies[0]);
            Assert.Equal("cumulonimbus", m.Skies[1]);
            Assert.Equal("lower limit 1680m", m.Skies[2]);
            Assert.Equal("broken", m.Skies[3]);
            Assert.Equal("towering cumulus", m.Skies[4]);
            Assert.Equal("lower limit 3230m", m.Skies[5]);
            Assert.Equal("overcast", m.Skies[6]);
            Assert.Equal("lower limit 4420m", m.Skies[7]);
        }

        [Theory]
        [InlineData("SCT", "scattered", 2)]
        [InlineData("BKN", "broken", 2)]
        [InlineData("OVC", "overcast", 2)]
        [InlineData("FEW", "few", 2)]
        [InlineData("NSC", "ceiling and visibility OK", 1)]
        [InlineData("NCD", "no clouds", 1)]
        public void SkyCover_0(string code, string name, int cnt)
        {
            var sc = new List<MetarSky>
            {
                new MetarSky { clouds = "", cloud_base_ft_agl = 5541, sky_cover = code }
            };
            var m = new MeteoRecordFactory(Language.ENG).CreateFrom(new Metar { metar_skies = sc });
            Assert.Equal(cnt, m.Skies.Count);
            Assert.Equal(name, m.Skies[0]);
        }

        [Theory]
        [InlineData("FEW", -1, "", "few; ")]
        [InlineData("SCT", -1, "", "scattered; ")]
        [InlineData("BKN", -1, "", "broken; ")]
        [InlineData("OVC", -1, "", "overcast; ")]
        [InlineData("///", -1, "CB", "cumulonimbus; ")]
        [InlineData("///", -1, "TCU", "towering cumulus; ")]
        [InlineData("BKN", 2500, "", "broken; lower limit 760m; ")]
        [InlineData("///", 1500, "", "lower limit 450m; ")]
        [InlineData("///", -1, "", "")]
        public void SkyCover_1(string cover, int agl, string clouds, string sky)
        {
            var sc = new List<MetarSky>
            {
                new MetarSky { clouds = clouds, cloud_base_ft_agl = agl, sky_cover = cover },
            };
            var m = new MeteoRecordFactory(Language.ENG).CreateFrom(new Metar { metar_skies = sc });
            //Assert.Equal(8, m.Skies.Count);
            string res = "";
            foreach (var s in m.Skies)
                res += s + "; ";
            Assert.Equal(sky, res);
        }




        [Theory]
        [InlineData("45R", "//////", "45 right", "no information (runway contaminated)")]
        [InlineData("88L", "SNOCLO", "88 left", "snow on the runway (AD closed)")]
        [InlineData("7C", "//99//", "7 central", "runway is being cleaned")]
        [InlineData("08", "CLRD60", "08", "runway cleared")]
        [InlineData("55", "155475", "55", "")]
        public void AdhesionRow_0(string aircode, string adhcode, string airname, string adhname)
        {
            var airs = new List<MetarAirStrip>();
            airs.Add(new MetarAirStrip { air_strip_code = aircode, adhesionrow = adhcode });
            var m = new MeteoRecordFactory(Language.ENG).CreateFrom(new Metar { metar_air_strips = airs });
            Assert.Single(m.AirStrips);
            var air = m.AirStrips.FirstOrDefault();
            Assert.Equal(adhname, air.AdhesionNote);
            Assert.Equal(airname, air.AirStripNo);
        }

        [Theory]
        [InlineData("45R", "//////", "45 right", "no information (runway contaminated)", "88L", "SNOCLO", "88 left", "snow on the runway (AD closed)")]
        [InlineData("7C", "//99//", "7 central", "runway is being cleaned", "08", "CLRD60", "08", "runway cleared")]
        public void AdhesionRow_1(string aircode, string adhcode, string airname, string adhname, string aircode2, string adhcode2, string airname2, string adhname2)
        {
            var airs = new List<MetarAirStrip>();
            airs.Add(new MetarAirStrip { air_strip_code = aircode, adhesionrow = adhcode });
            airs.Add(new MetarAirStrip { air_strip_code = aircode2, adhesionrow = adhcode2 });
            var m = new MeteoRecordFactory(Language.ENG).CreateFrom(new Metar { metar_air_strips = airs });
            Assert.Equal(2, m.AirStrips.Count);
            var air = m.AirStrips.FirstOrDefault();
            Assert.Equal(adhname, air.AdhesionNote);
            Assert.Equal(airname, air.AirStripNo);
            var air2 = m.AirStrips.LastOrDefault();
            Assert.Equal(adhname2, air2.AdhesionNote);
            Assert.Equal(airname2, air2.AirStripNo);
        }

        [Theory]
        [InlineData("SHRA", "showers rain (medium)")]
        [InlineData("+SHRA", "showers rain (heavy)")]
        [InlineData("-SHRA", "showers rain (light)")]
        [InlineData("SH", "shower (medium)")]
        [InlineData("VCSH", "in the vicinity of the airfield shower (medium)")]
        [InlineData("-VCSH", "in the vicinity of the airfield shower (light)")]
        [InlineData("+VCSH", "in the vicinity of the airfield shower (heavy)")]
        [InlineData("+SH", "shower (heavy)")]
        [InlineData("FU", "fume/smoke")]
        [InlineData("MIFG", "shallow fog")]
        [InlineData("DS", "dust storm (medium)")]
        [InlineData("VCDS", "in the vicinity of the airfield dust storm (medium)")]
        public void Conditions_0(string code, string name)
        {
            var m = new MeteoRecordFactory(Language.ENG).CreateFrom(new Metar { wx_string = code });
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
            var s = new List<MetarSky>();
            s.Add(new MetarSky { clouds = "CB", sky_cover = "OVC", cloud_base_ft_agl = 1600 });
            var t = new MetarTrend { trend_type = TrendType.BECMG, time_at = DateTime.ParseExact("11:00", "HH:mm", null), wx_string = "SH", metar_skies = s, visibility_statute_m = 5000 };
            var m = new MeteoRecordFactory(Language.ENG).CreateFrom(new Metar { trends = { t } });
            Assert.Single(m.Trends);
            Assert.Equal("visibility 5000m, shower (medium), cloud overcast cumulonimbus lower limit 480m", m.Trends.First().Info);
        }

        [Fact]
        public void Trends_1()
        {
            var t = new MetarTrend { trend_type = TrendType.BECMG, time_from = DateTime.ParseExact("11:00", "HH:mm", null), time_to = DateTime.ParseExact("12:00", "HH:mm", null), wx_string = "FU", vertical_visiblity_ft = 500 };
            var m = new MeteoRecordFactory(Language.ENG).CreateFrom(new Metar { trends = { t } });
            Assert.Single(m.Trends);
            Assert.Equal("fume/smoke, vertical visibility 150m", m.Trends.First().Info);
        }

        [Fact]
        public void Trends_2()
        {
            var t = new MetarTrend { trend_type = TrendType.TEMPO, wind_changingdir = true, wind_gust = 20, wind_speed = 10 };
            var m = new MeteoRecordFactory(Language.ENG).CreateFrom(new Metar { trends = { t } });
            Assert.Single(m.Trends);
            Assert.Equal("Temporary wind variable 10m/s gusts 20m/s", m.Trends.First().Info);
        }

        [Fact]
        public void Trends_3()
        {
            var t = new MetarTrend { trend_type = TrendType.BECMG, time_at = DateTime.ParseExact("11:00", "HH:mm", null), wx_string = "NSW" };
            var m = new MeteoRecordFactory(Language.ENG).CreateFrom(new Metar { trends = { t } });
            Assert.Single(m.Trends);
            Assert.Equal("cessation of significant weather", m.Trends.First().Info);
        }

        [Fact]
        public void Trends_4()
        {
            var t = new MetarTrend { trend_type = TrendType.BECMG, time_from = DateTime.ParseExact("11:00", "HH:mm", null), time_to = DateTime.ParseExact("12:00", "HH:mm", null), vertical_visiblity_ft = 300 };
            var m = new MeteoRecordFactory(Language.ENG).CreateFrom(new Metar { trends = { t } });
            Assert.Single(m.Trends);
            Assert.Equal("vertical visibility 90m", m.Trends.First().Info);
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
            var m = new MeteoRecordFactory(Language.ENG).CreateFrom(new Metar { trends = { t } });
            Assert.Single(m.Trends);
            Assert.Equal(timefrom, m.Trends.First().TimeFrom);
            Assert.Equal(timeto, m.Trends.First().TimeTo);
            Assert.Equal(timeat, m.Trends.First().TimeAt);
        }


    }
}
