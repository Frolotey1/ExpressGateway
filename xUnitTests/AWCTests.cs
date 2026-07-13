using MeteoLib.AviationWeather;
using MeteoLib.AviationWeather.AwcSource;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace xUnitTests
{
    public class AWCTests
    {

        [Fact]
        public void Test_0()
        {
            var x = GetXML_0();
            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var metars = AWC.ParseMetarData(x);

            Assert.Single(metars);
            var m = metars.First();

            Assert.Equal(4, m.dewpoint_c);
            Assert.False(m.dewpoint_c_sing);
            Assert.Equal(5, m.temp_c);
            Assert.True(m.temp_c_sing);
            Assert.Equal("UHPP", m.station_id);
            Assert.Equal("-SHRA -SHSN", m.wx_string);
            Assert.Equal(9999, m.visibility_statute_m);
            Assert.Equal(90, m.wind_dir_degrees);
            Assert.Equal(2, m.wind_speed_kt);
            Assert.Equal(7, m.wind_gust_kt);
            Assert.Equal(2, m.metar_skies.Count);
            Assert.Equal("BKN", m.metar_skies.First().sky_cover);
            Assert.Equal(4600, m.metar_skies.First().cloud_base_ft_agl);
            Assert.Equal("CB", m.metar_skies.First().clouds);
            Assert.Equal("BKN", m.metar_skies.Last().sky_cover);
            Assert.Equal(13000, m.metar_skies.Last().cloud_base_ft_agl);
            Assert.Equal("TCU", m.metar_skies.Last().clouds);
            Assert.Equal(755, m.atm_preasure);
        }

        [Fact]
        public void Test_1()
        {
            var x = GetXML_1();
            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var metars = AWC.ParseMetarData(x);

            Assert.Single(metars);
            var m = metars.First();

            Assert.Single(m.metar_skies);
            Assert.Equal("CAVOK", m.metar_skies.First().sky_cover);
        }

        [Fact]
        public void Test_2()
        {
            var x = GetXML_2();
            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var metars = AWC.ParseMetarData(x);

            Assert.Single(metars);
            var m = metars.First();

            Assert.Null(m.dewpoint_c);
            Assert.Null(m.temp_c);
            Assert.Equal(10000, m.visibility_statute_m);
            Assert.Equal(80, m.wind_dir_degrees);
            Assert.Equal(12, m.wind_speed_kt);
            Assert.Null(m.wind_gust_kt);
            Assert.Null(m.atm_preasure);
        }

        [Fact]
        public void Cavok_0()
        {
            var xml = AddHeaders($"<raw_text>UHPP 080430Z VRB01MPS CAVOK BKN046CB BKN130TCU 05/M04 Q1011 R26R/010060 NOSIG RMK MT OBSC QFE755/1007</raw_text>");
            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var metars = AWC.ParseMetarData(xml);

            Assert.Single(metars);
            var m = metars.First();
            Assert.Equal(10000, m.visibility_statute_m);
        }

        [Fact]
        public void Visiblity_0()
        {
            var xml = AddHeaders($"<raw_text>UHPP 080430Z VRB01MPS 0099 BKN046CB BKN130TCU 05/M04 Q1011 R26R/010060 NOSIG RMK MT OBSC QFE755/1007</raw_text>");
            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var metars = AWC.ParseMetarData(xml);

            Assert.Single(metars);
            var m = metars.First();
            Assert.Equal(99, m.visibility_statute_m);
        }

        [Theory]
        [InlineData("R26R/010060", 0.6, "26R", "010060")]
        [InlineData("R34L/CLRD70", 0.7, "34L", "CLRD70")]
        [InlineData("R3C/010035", 0.35, "3C", "010035")]
        [InlineData("R99///////", 0, "99", "//////")]
        [InlineData("R45/SNOCLO", 0, "45", "SNOCLO")]
        [InlineData("R4C///99//", 0, "4C", "//99//")]
        [InlineData("R85L/CLRD//", 0, "85L", "CLRD//")]
        [InlineData("R26L/81//60", 0.6, "26L", "81//60")]
        public void Adhesion_0(string adhesionBlock, double adhesion, string stripcode, string adhrow)
        {
            var xml = AddHeaders($"<raw_text>UHPP 080430Z VRB01MPS 9999 BKN046CB BKN130TCU 05/M04 Q1011 {adhesionBlock} NOSIG RMK MT OBSC QFE755/1007</raw_text>");
            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var metars = AWC.ParseMetarData(xml);

            Assert.Single(metars);
            var m = metars.First();
            Assert.Single(m.metar_air_strips);
            var strip = m.metar_air_strips.FirstOrDefault();
            Assert.Equal(adhesion, strip.adhesion);
            Assert.Equal(adhrow, strip.adhesionrow);
            Assert.Equal(stripcode, strip.air_strip_code);
           
        }

        [Theory]
        [InlineData("01/02", 1, true, 2, true)]
        [InlineData("01/M02", 1, true, 2, false)]
        [InlineData("M30/M02", 30, false, 2, false)]
        [InlineData("00/M00", 0, true, 0, false)]
        [InlineData("M00/00", 0, false, 0, true)]
        [InlineData("M00/M00", 0, false, 0, false)]
        public void Temp_0(string tempBlock, double temp, bool temp_sign, double dewpoint, bool dewpoint_sign)
        {
            var xml = AddHeaders($"<raw_text>UHPP 080430Z VRB01MPS 9999 BKN046CB BKN130TCU {tempBlock} Q1011 R26R/010060 NOSIG RMK MT OBSC QFE755/1007</raw_text>");
            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var metars = AWC.ParseMetarData(xml);

            Assert.Single(metars);
            var m = metars.First();
            Assert.Equal(temp, m.temp_c);
            Assert.Equal(temp_sign, m.temp_c_sing);
            Assert.Equal(dewpoint, m.dewpoint_c);
            Assert.Equal(dewpoint_sign, m.dewpoint_c_sing);
        }



        [Theory]
        [InlineData("R26R/010060 R34L/CLRD70", 0.6, "26R", "010060", 0.7, "34L", "CLRD70")]
        [InlineData("R3C/010035 R99///////", 0.35, "3C", "010035", 0, "99", "//////")]
        [InlineData("R45/SNOCLO R4C///99//", 0, "45", "SNOCLO", 0, "4C", "//99//")]
        public void Adhesion_1(string adhesionBlock, double adhesion1, string stripcode1, string adhrow1, double adhesion2, string stripcode2, string adhrow2)
        {
            var xml = AddHeaders($"<raw_text>UHPP 080430Z VRB01MPS 9999 BKN046CB BKN130TCU 05/M04 Q1011 {adhesionBlock} NOSIG RMK MT OBSC QFE755/1007</raw_text>");
            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var metars = AWC.ParseMetarData(xml);

            Assert.Single(metars);
            var m = metars.First();
            Assert.Equal(2, m.metar_air_strips.Count);
            var strip = m.metar_air_strips.FirstOrDefault();
            Assert.Equal(adhesion1, strip.adhesion);
            Assert.Equal(adhrow1, strip.adhesionrow);
            Assert.Equal(stripcode1, strip.air_strip_code);
            var secondstrip = m.metar_air_strips.LastOrDefault();
            Assert.Equal(adhesion2, secondstrip.adhesion);
            Assert.Equal(adhrow2, secondstrip.adhesionrow);
            Assert.Equal(stripcode2, secondstrip.air_strip_code);
        }

        [Fact]
        public void SkyCond_0()
        {
            var xml = AddHeaders(@"<raw_text>UHPP 080430Z VRB01MPS 9999 FEW042CB SCT150 OVC230TCU 05/M04 Q1011 R34L/CLRD70 NOSIG RMK MT OBSC QFE755/1007</raw_text>");
            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var metars = AWC.ParseMetarData(xml);

            Assert.Single(metars);
            var m = metars.First();
            Assert.Equal(3, m.metar_skies.Count);

            var item = m.metar_skies[0];
            Assert.Equal("FEW", item.sky_cover);
            Assert.Equal(4200, item.cloud_base_ft_agl);
            Assert.Equal("CB", item.clouds);
            item = m.metar_skies[1];
            Assert.Equal("SCT", item.sky_cover);
            Assert.Equal(15000, item.cloud_base_ft_agl);
            Assert.Equal("", item.clouds);
            item = m.metar_skies[2];
            Assert.Equal("OVC", item.sky_cover);
            Assert.Equal(23000, item.cloud_base_ft_agl);
            Assert.Equal("TCU", item.clouds);
        }

        [Theory]
        [InlineData("FEW///", "FEW", -1, "")]
        [InlineData("SCT///", "SCT", -1, "")]
        [InlineData("BKN///", "BKN", -1, "")]
        [InlineData("OVC///", "OVC", -1, "")]
        [InlineData("/////CB", "///", -1, "CB")]
        [InlineData("/////TCU", "///", -1, "TCU")]
        [InlineData("BKN025///", "BKN", 2500, "")]
        [InlineData("///015", "///", 1500, "")]
        [InlineData("//////", "///", -1, "")]
        public void SkyCond_1(string raw, string cover, int agl, string clouds)
        {
            var xml = AddHeaders($"<raw_text>UHPP 080430Z VRB01MPS 9999 {raw} 05/M04 Q1011 R34L/CLRD70 NOSIG RMK MT OBSC QFE755/1007</raw_text>");
            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var metars = AWC.ParseMetarData(xml);

            Assert.Single(metars);
            var m = metars.First();
            Assert.Single(m.metar_skies);

            var item = m.metar_skies[0];
            Assert.Equal(cover, item.sky_cover);
            Assert.Equal(agl, item.cloud_base_ft_agl);
            Assert.Equal(clouds, item.clouds);
        }

        [Fact]
        public void wind_0()
        {
            var xml = AddHeaders(@"<raw_text>UHPP 080430Z VRB01MPS 9999 FEW042CB SCT150 OVC230TCU 05/M04 Q1011 R34L/CLRD70 NOSIG RMK MT OBSC QFE755/1007</raw_text>
                                   <wind_dir_degrees>90</wind_dir_degrees> 
                                   <wind_speed_kt>2</wind_speed_kt>");
            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var metars = AWC.ParseMetarData(xml);

            Assert.Single(metars);
            var m = metars.First();
            Assert.True(m.wind_changingdir);
            Assert.Equal(2, m.wind_speed_kt);
        }

        [Fact]
        public void wind_1()
        {
            var xml = AddHeaders(@"<raw_text>UHPP 080430Z 00000MPS 9999 FEW042CB SCT150 OVC230TCU 05/M04 Q1011 R34L/CLRD70 NOSIG RMK MT OBSC QFE755/1007</raw_text>
                                   <wind_dir_degrees>90</wind_dir_degrees> 
                                   <wind_speed_kt>0</wind_speed_kt>");
            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var metars = AWC.ParseMetarData(xml);

            Assert.Single(metars);
            var m = metars.First();
            Assert.False(m.wind_changingdir);
            Assert.Equal(0, m.wind_speed_kt);
        }

        [Fact]
        public void wind_2()
        {
            var xml = AddHeaders(@"<raw_text>UHPP 080430Z 09002MPS 9999 FEW042CB SCT150 OVC230TCU 05/M04 Q1011 R34L/CLRD70 NOSIG RMK MT OBSC QFE755/1007</raw_text>
                                   <wind_dir_degrees>90</wind_dir_degrees> 
                                   <wind_speed_kt>4</wind_speed_kt>");
            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var metars = AWC.ParseMetarData(xml);

            Assert.Single(metars);
            var m = metars.First();
            Assert.False(m.wind_changingdir);
            Assert.Equal(4, m.wind_speed_kt);
            Assert.Equal(90, m.wind_dir_degrees);
        }

        [Theory]
        [InlineData("MT OBSC")]
        [InlineData("MAST OBSC")]
        [InlineData("OBST OBSC")]
        public void mountain_0(string mountain)
        {
            var xml = AddHeaders($"<raw_text>UHPP 080430Z 09002MPS 9999 FEW042CB SCT150 OVC230TCU 05/M04 Q1011 R34L/CLRD70 NOSIG RMK {mountain} QFE755/1007</raw_text>");
            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var metars = AWC.ParseMetarData(xml);

            Assert.Single(metars);
            var m = metars.First();
            Assert.Equal(mountain, m.mountain_visiblity);
        }

        [Fact]
        public void raw_0()
        {
            var xml = AddHeaders(@"<raw_text>UHPP 080430Z 09002MPS 9999 FEW042CB SCT150 OVC230TCU 05/M04 Q1011 R34L/CLRD70 NOSIG RMK MT OBSC QFE755/1007</raw_text>");
            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var metars = AWC.ParseMetarData(xml);

            Assert.Single(metars);
            var m = metars.First();
            Assert.Equal("UHPP 080430Z 09002MPS 9999 FEW042CB SCT150 OVC230TCU 05/M04 Q1011 R34L/CLRD70", m.raw_main);
            Assert.Equal("RMK MT OBSC QFE755/1007", m.raw_rmk);
            Assert.Empty(m.trends);
        }

        [Fact]
        public void raw_1()
        {
            var xml = AddHeaders(@"<raw_text>UHPP 080430Z 09002MPS 9999 FEW042CB SCT150 OVC230TCU 05/M04 Q1011 R34L/CLRD70 RMK MT OBSC QFE755/1007</raw_text>");
            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var metars = AWC.ParseMetarData(xml);

            Assert.Single(metars);
            var m = metars.First();
            Assert.Equal("UHPP 080430Z 09002MPS 9999 FEW042CB SCT150 OVC230TCU 05/M04 Q1011 R34L/CLRD70", m.raw_main);
            Assert.Equal("RMK MT OBSC QFE755/1007", m.raw_rmk);
            Assert.Empty(m.trends);
        }

        [Fact]
        public void raw_2()
        {
            var xml = AddHeaders(@"<raw_text>UHPP 080430Z 09002MPS 9999 FEW042CB SCT150 OVC230TCU 05/M04 Q1011 R34L/CLRD70 BECMG TL1700 0800 FG BECMG AT1800 9999 NSW RMK MT OBSC QFE755/1007</raw_text>");
            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var metars = AWC.ParseMetarData(xml);

            Assert.Single(metars);
            var m = metars.First();
            Assert.Equal("UHPP 080430Z 09002MPS 9999 FEW042CB SCT150 OVC230TCU 05/M04 Q1011 R34L/CLRD70", m.raw_main);
            Assert.Equal("RMK MT OBSC QFE755/1007", m.raw_rmk);
            Assert.Equal(2, m.trends.Count);
            Assert.Single(m.trends.Where(p => p.trand_raw == "BECMG AT1800 9999 NSW "));
            Assert.Single(m.trends.Where(p => p.trand_raw == "BECMG TL1700 0800 FG "));  
        }

        [Fact]
        public void raw_3()
        {
            var xml = AddHeaders(@"<raw_text>UHPP 080430Z 09002MPS 9999 FEW042CB SCT150 OVC230TCU 05/M04 Q1011 R34L/CLRD70</raw_text>");
            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var metars = AWC.ParseMetarData(xml);

            Assert.Single(metars);
            var m = metars.First();
            Assert.Equal("UHPP 080430Z 09002MPS 9999 FEW042CB SCT150 OVC230TCU 05/M04 Q1011 R34L/CLRD70", m.raw_main);
            Assert.Equal("", m.raw_rmk);
            Assert.Empty(m.trends);
        }

        [Fact]
        public void TrendVisiblity_0()
        {
            var xml = AddHeaders($"<raw_text>UHPP 080430Z 09002MPS 9999 FEW042CB SCT150 OVC230TCU 05/M04 Q1011 R34L/CLRD70 TEMPO FG RMK MT OBSC QFE755/1007</raw_text>");
            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var metars = AWC.ParseMetarData(xml);

            Assert.Single(metars);
            var m = metars.First();
            Assert.Single(m.trends);
            var t = m.trends.First();
            Assert.Null(t.visibility_statute_m);
            Assert.Null(t.time_from);
            Assert.Null(t.time_at);
        }

        [Theory]
        [InlineData("0000")]
        [InlineData("0010")]
        [InlineData("8000")]
        [InlineData("9999")]
        public void TrendVisiblity_1(string raw_vis)
        {
            var xml = AddHeaders($"<raw_text>UHPP 080430Z 09002MPS 9999 FEW042CB SCT150 OVC230TCU 05/M04 Q1011 R34L/CLRD70 TEMPO {raw_vis} FG RMK MT OBSC QFE755/1007</raw_text>");
            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var metars = AWC.ParseMetarData(xml);

            Assert.Single(metars);
            var m = metars.First();
            Assert.Single(m.trends);
            var t = m.trends.First();
            Assert.Equal(int.Parse(raw_vis), t.visibility_statute_m);
            Assert.Null(t.time_from);
            Assert.Null(t.time_at);
        }

        [Theory]
        [InlineData("TL0000", "00:00")]
        [InlineData("TL1000", "10:00")]
        [InlineData("TL1700", "17:00")]
        [InlineData("TL2359", "23:59")]
        public void TrendTime_0(string raw_time, string time)
        {
            var xml = AddHeaders($"<raw_text>UHPP 080430Z 09002MPS 9999 FEW042CB SCT150 OVC230TCU 05/M04 Q1011 R34L/CLRD70 BECMG {raw_time} 0800 FG RMK MT OBSC QFE755/1007</raw_text>");
            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var metars = AWC.ParseMetarData(xml);

            Assert.Single(metars);
            var m = metars.First();
            Assert.Single(m.trends);
            var t = m.trends.First();
            Assert.Equal(DateTime.ParseExact(time, "HH:mm", null), t.time_to);
            Assert.Null(t.time_from);
            Assert.Null(t.time_at);

        }

        [Theory]
        [InlineData("FM0000", "00:00")]
        [InlineData("FM1000", "10:00")]
        [InlineData("FM1700", "17:00")]
        [InlineData("FM2359", "23:59")]
        public void TrendTime_1(string raw_time, string time)
        {
            var xml = AddHeaders($"<raw_text>UHPP 080430Z 09002MPS 9999 FEW042CB SCT150 OVC230TCU 05/M04 Q1011 R34L/CLRD70 BECMG {raw_time} 0800 FG RMK MT OBSC QFE755/1007</raw_text>");
            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var metars = AWC.ParseMetarData(xml);

            Assert.Single(metars);
            var m = metars.First();
            Assert.Single(m.trends);
            var t = m.trends.First();
            Assert.Equal(DateTime.ParseExact(time, "HH:mm", null), t.time_from);
            Assert.Null(t.time_to);
            Assert.Null(t.time_at);

        }

        [Theory]
        [InlineData("AT2400", "00:00")]
        [InlineData("AT1000", "10:00")]
        [InlineData("AT1700", "17:00")]
        [InlineData("AT2359", "23:59")]
        public void TrendTime_2(string raw_time, string time)
        {
            var xml = AddHeaders($"<raw_text>UHPP 080430Z 09002MPS 9999 FEW042CB SCT150 OVC230TCU 05/M04 Q1011 R34L/CLRD70 BECMG {raw_time} 0800 FG RMK MT OBSC QFE755/1007</raw_text>");
            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var metars = AWC.ParseMetarData(xml);

            Assert.Single(metars);
            var m = metars.First();
            Assert.Single(m.trends);
            var t = m.trends.First();
            Assert.Equal(DateTime.ParseExact(time, "HH:mm", null), t.time_at);
            Assert.Null(t.time_from);
            Assert.Null(t.time_to);
        }


        [Theory]
        [InlineData("31005MPS", 310, 5, null, false)]
        [InlineData("31005G10MPS", 310, 5, 10, false)]
        [InlineData("VRB17MPS", null, 17, null, true)]
        [InlineData("240P49MPS", 240, 50, null, false)]
        [InlineData("00000MPS", 0, 0, null, false)]
        public void TrendWind_0(string raw_time, int? dir, int? speed, int? gust, bool? change)
        {
            var xml = AddHeaders($"<raw_text>UHPP 080430Z 09002MPS 9999 FEW042CB SCT150 OVC230TCU 05/M04 Q1011 R34L/CLRD70 BECMG {raw_time} 0800 FG RMK MT OBSC QFE755/1007</raw_text>");
            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var metars = AWC.ParseMetarData(xml);

            Assert.Single(metars);
            var m = metars.First();
            Assert.Single(m.trends);
            var t = m.trends.First();
            Assert.Equal(dir, t.wind_dir_degrees);
            Assert.Equal(speed, t.wind_speed);
            Assert.Equal(gust, t.wind_gust);
            Assert.Equal(change, t.wind_changingdir);
        }



        [Theory]
        [InlineData("SQ RA TS")]
        [InlineData("SQ +TSRA")]
        [InlineData("SQ -TSRA")]
        [InlineData("-SQ RA")]
        [InlineData("SQ FZSHRA")]
        [InlineData("SQ +FZSHRA")]
        [InlineData("FLASHES")]
        [InlineData("SQ")]
        [InlineData("SQ FLASHES")]
        [InlineData("NSW")]
        public void TrendWX_0(string raw)
        {
            var xml = AddHeaders($"<raw_text>UHPP 080430Z 09002MPS 9999 FEW042CB SCT150 OVC230TCU 05/M04 Q1011 R34L/CLRD70 BECMG 0800 {raw} RMK MT OBSC QFE755/1007</raw_text>");
            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var metars = AWC.ParseMetarData(xml);

            Assert.Single(metars);
            var m = metars.First();
            Assert.Single(m.trends);
            var t = m.trends.First();
            Assert.Equal(raw, t.wx_string);

        }


        [Theory]
        [InlineData("VV000", 0)]
        [InlineData("VV100", 10000)]
        [InlineData("VV987", 98700)]
        [InlineData("", null)]
        public void TrendVertVis_0(string vis, int? val)
        {
            var xml = AddHeaders($"<raw_text>UHPP 080430Z 09002MPS 9999 FEW042CB SCT150 OVC230TCU 05/M04 Q1011 R34L/CLRD70 BECMG {vis} 0800 FG RMK MT OBSC QFE755/1007</raw_text>");
            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var metars = AWC.ParseMetarData(xml);

            Assert.Single(metars);
            var m = metars.First();
            Assert.Single(m.trends);
            var t = m.trends.First();
            Assert.Equal(val, t.vertical_visiblity_ft);

        }

        [Theory]
        [InlineData("BKN016CB", "BKN", 1600, "CB")]
        [InlineData("OVC005", "OVC", 500, "")]
        [InlineData("FEW216TCU", "FEW", 21600, "TCU")]
        [InlineData("OVX", "OVX", 0, "")]
        [InlineData("NSC", "NSC", 0, "")]
        public void TrendVerSkies_0(string raw, string cover, int agl, string clouds)
        {
            var xml = AddHeaders($"<raw_text>UHPP 080430Z 09002MPS 9999 FEW042CB SCT150 OVC230TCU 05/M04 Q1011 R34L/CLRD70 BECMG {raw} 0800 FG RMK MT OBSC QFE755/1007</raw_text>");
            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var metars = AWC.ParseMetarData(xml);

            Assert.Single(metars);
            var m = metars.First();
            Assert.Single(m.trends);
            var t = m.trends.First();
            Assert.Single(t.metar_skies);
            var s = t.metar_skies.First();
            Assert.Equal(cover, s.sky_cover);
            Assert.Equal(agl, s.cloud_base_ft_agl);
            Assert.Equal(clouds, s.clouds);

        }

        [Fact]
        public void TrendVerSkies_1()
        {
            var raw = "BKN016CB OVC005";

            var xml = AddHeaders($"<raw_text>UHPP 080430Z 09002MPS 9999 FEW042CB SCT150 OVC230TCU 05/M04 Q1011 R34L/CLRD70 BECMG {raw} 0800 FG RMK MT OBSC QFE755/1007</raw_text>");
            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var metars = AWC.ParseMetarData(xml);

            Assert.Single(metars);
            var m = metars.First();
            Assert.Single(m.trends);
            var t = m.trends.First();
            Assert.Equal(2, t.metar_skies.Count);
            var s1 = t.metar_skies.Where(p => p.sky_cover == "BKN");
            Assert.Single(s1);
            Assert.Equal("BKN", s1.First().sky_cover);
            Assert.Equal(1600, s1.First().cloud_base_ft_agl);
            Assert.Equal("CB", s1.First().clouds);
            var s2 = t.metar_skies.Where(p => p.sky_cover == "OVC");
            Assert.Single(s2);
            Assert.Equal("OVC", s2.First().sky_cover);
            Assert.Equal(500, s2.First().cloud_base_ft_agl);
            Assert.Equal("", s2.First().clouds);

        }


        [Fact]
        public void TrendVerSkies_2()
        {
            var raw = "";

            var xml = AddHeaders($"<raw_text>UHPP 080430Z 09002MPS 9999 FEW042CB SCT150 OVC230TCU 05/M04 Q1011 R34L/CLRD70 BECMG {raw} 0800 FG RMK MT OBSC QFE755/1007</raw_text>");
            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var metars = AWC.ParseMetarData(xml);

            Assert.Single(metars);
            var m = metars.First();
            Assert.Single(m.trends);
            var t = m.trends.First();
            Assert.Empty(t.metar_skies);
        }


        [Fact]
        public void TrendTest_0()
        {
            var xml = AddHeaders($"<raw_text>UUEE 051500Z 13005MPS 9999 -SHRA SCT051CB 21/18 Q1014 R06R/290051 R06C/290051 BECMG TL1700 0800 FG BECMG AT1800 9999 NSW</raw_text>");
            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var metars = AWC.ParseMetarData(xml);

            Assert.Single(metars);
            var m = metars.First();
            Assert.Equal(2,m.trends.Count);
            var t = m.trends.Last();
            Assert.Equal(9999, t.visibility_statute_m);
            Assert.Empty(t.metar_skies);
        }


        [Fact]
        public void TrendTest_1()
        {
            var xml = AddHeaders($"<raw_text>UUEE 051500Z 13005MPS 9999 -SHRA SCT051CB 21/18 Q1014 R06R/290051 R06C/290051 TEMPO 2900 -TSRA BKN020CB VV003</raw_text>");
            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var metars = AWC.ParseMetarData(xml);

            Assert.Single(metars);
            var m = metars.First();
            Assert.Single(m.trends);
            var t = m.trends.First();
            Assert.Equal(300, t.vertical_visiblity_ft);
        }




        public static string AddHeaders(string xml)
        {
            return @"<response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" version=""1.2"" xsi:noNamespaceSchemaLocation=""http://www.aviationweather.gov/static/adds/schema/metar1_2.xsd"">
                        <request_index>322307207</request_index>
                        <data_source name=""metars""/>
                        <request type=""retrieve""/>
                        <errors/>
                        <warnings/>
                        <time_taken_ms> 37 </time_taken_ms>
                        <data num_results=""1""> 
                          <METAR>
                          " + xml + @"
                          </METAR>
                        </data>
                    </response>";

        }

        public static string GetXML_0()
        {
            var x = @"      <raw_text>UHPP 080430Z VRB01MPS 9999 BKN046CB BKN130TCU 05/M04 Q1011 R34L/CLRD70 NOSIG RMK MT OBSC QFE755/1007</raw_text> 
                            <station_id>UHPP</station_id> 
                            <observation_time>2022-04-08T04:30:00Z</observation_time> 
                            <latitude>53.17</latitude>
                            <longitude>158.45</longitude>
                            <temp_c>5.0</temp_c>
                            <dewpoint_c>-4.0</dewpoint_c>
                            <wind_dir_degrees>90</wind_dir_degrees> 
                            <wind_speed_kt>2</wind_speed_kt> 
                            <wind_gust_kt>7</wind_gust_kt> 
                            <visibility_statute_mi>6.21</visibility_statute_mi>
                            <altim_in_hg>29.852362</altim_in_hg>
                            <quality_control_flags>
                            <no_signal>TRUE</no_signal>
                            </quality_control_flags>
                            <wx_string>-SHRA -SHSN</wx_string> 
                            <sky_condition sky_cover=""BKN"" cloud_base_ft_agl=""4600""/> 
                            <sky_condition sky_cover=""BKN"" cloud_base_ft_agl=""13000""/> 
                            <flight_category>VFR</flight_category>
                            <metar_type>METAR</metar_type>
                            <elevation_m>33.0</elevation_m> ";
            return AddHeaders(x);
        }

        public static string GetXML_1()
        {
            var x = @"      <raw_text>UWWW 020600Z 02005MPS CAVOK 24/14 Q1016 R33/CLRD60 NOSIG RMK QFE751/1001</raw_text>
                            <station_id>UWWW</station_id>
                            <observation_time>2022-08-02T06:00:00Z</observation_time>
                            <latitude>53.5</latitude>
                            <longitude>50.17</longitude>
                            <temp_c>24.0</temp_c>
                            <dewpoint_c>14.0</dewpoint_c>
                            <wind_dir_degrees>20</wind_dir_degrees>
                            <wind_speed_kt>10</wind_speed_kt>
                            <visibility_statute_mi>6.21</visibility_statute_mi>
                            <altim_in_hg>30.0</altim_in_hg>
                            <quality_control_flags>
                            <no_signal>TRUE</no_signal>
                            </quality_control_flags>
                            <sky_condition sky_cover=""CAVOK""/>
                            <flight_category> VFR </flight_category>
                            <metar_type> METAR </metar_type>
                            <elevation_m> 124.0 </elevation_m> ";
            return AddHeaders(x);
        }

        public static string GetXML_2()
        {
            var x = @"      <raw_text>UWOO 221030Z 08006MPS 050V110 CAVOK ///// Q//// R08/////// NOSIG RMK QFE////////</raw_text>
                            <station_id>UWOO</station_id>
                            <observation_time>2022-08-22T10:30:00Z</observation_time>
                            <latitude>51.8</latitude>
                            <longitude>55.47</longitude>
                            <wind_dir_degrees>80</wind_dir_degrees>
                            <wind_speed_kt>12</wind_speed_kt>
                            <visibility_statute_mi>6.21</visibility_statute_mi>
                            <quality_control_flags>
                            <no_signal>TRUE</no_signal>
                            </quality_control_flags>
                            <sky_condition sky_cover=""CAVOK""/>
                            <flight_category> VFR </flight_category>
                            <metar_type> METAR </metar_type>
                            <elevation_m> 90.0 </elevation_m> ";
            return AddHeaders(x);
        }



    }
}
