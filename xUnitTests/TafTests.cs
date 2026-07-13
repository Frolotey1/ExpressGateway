using Daocore.OracleEngine;
using MeteoLib;
using MeteoLib.AviationWeather;
using MeteoLib.AviationWeather.AwcSource;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Linq;
using System.Text.Json;
using System.Xml;
using Xunit;
using static MeteoLib.SourceText;

namespace xUnitTests
{
    public class TafTests
    {
        [Fact]
        public void TafTest1()
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load("../../../tafExamples/1.xml");
            var xml = xDoc.OuterXml;

            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var tafs = AWC.ParseTafData(xml);

            Assert.Single(tafs);
            var m = tafs.First();

            var tafRecord = new MeteoRecordFactory(Language.RUS).CreateFrom(m);
            Assert.Equal("TAF USSS 210445Z 2106/2206 25003G11MPS 9999 SCT030", tafRecord.MainRaw);
            Assert.Equal("USSS", tafRecord.StationId);

            Assert.Equal(new DateTime(2022, 11, 21, 4, 45, 0, DateTimeKind.Utc), tafRecord.IssueTime);

            Assert.Equal(new DateTime(2022, 11, 21, 6, 0, 0, DateTimeKind.Utc), tafRecord.Start);
            Assert.Equal(new DateTime(2022, 11, 22, 6, 0, 0, DateTimeKind.Utc), tafRecord.End);

            WindRecord wind = new()
            {
                WindSpeed = 3,
                WindGust = 11,
                WindDirection = "запад"
            };
            Assert.Equal(wind.WindDirection, tafRecord.Wind.WindDirection);
            Assert.Equal(wind.WindSpeed, tafRecord.Wind.WindSpeed);
            Assert.Equal(wind.WindGust, tafRecord.Wind.WindGust);

            Assert.Equal("более 10000м", tafRecord.Visibility);

            Assert.Equal("разбросанная, нижн. гран. 910м", string.Join(", ", tafRecord.MainSkies));

            Assert.Single(tafRecord.Trends);

            var trend = tafRecord.Trends[0];
            Assert.Equal("FM211400 27003G08MPS 6000 SCT016", trend.TafText);

            Assert.Equal(new DateTime(2022, 11, 21, 14, 0, 0, DateTimeKind.Utc), trend.TimeFrom);

            WindRecord trendWind = new()
            {
                WindSpeed = 3,
                WindGust = 8,
                WindDirection = "запад"
            };
            Assert.Equal(trendWind.WindDirection, trend.Wind.WindDirection);
            Assert.Equal(trendWind.WindSpeed, trend.Wind.WindSpeed);
            Assert.Equal(trendWind.WindGust, trend.Wind.WindGust);

            Assert.Equal("ветер запад 3м/с с порывами 8м/с, видимость 6000м, облачность разбросанная нижн. гран. 480м", trend.Info);
        }
        [Fact]
        public void TafTest2()
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load("../../../tafExamples/2.xml");
            var xml = xDoc.OuterXml;

            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var tafs = AWC.ParseTafData(xml);

            Assert.Single(tafs);
            var m = tafs.First();

            var tafRecord = new MeteoRecordFactory(Language.RUS).CreateFrom(m);
            Assert.Equal("TAF UWGG 210155Z 2103/2203 05003MPS 6000 -SN OVC006 TXM04/2112Z TNM08/2103Z", tafRecord.MainRaw);
            Assert.Equal("UWGG", tafRecord.StationId);

            Assert.Equal(new DateTime(2022, 11, 21, 1, 55, 0, DateTimeKind.Utc), tafRecord.IssueTime);

            Assert.Equal(new DateTime(2022, 11, 21, 3, 0, 0, DateTimeKind.Utc), tafRecord.Start);
            Assert.Equal(new DateTime(2022, 11, 22, 3, 0, 0, DateTimeKind.Utc), tafRecord.End);

            WindRecord wind = new()
            {
                WindSpeed = 3d,
                WindGust = null,
                WindDirection = "северо-восток"
            };
            //Assert.Equal(wind, tafRecord.Wind);
            Assert.Equal(wind.WindDirection, tafRecord.Wind.WindDirection);
            Assert.Equal(wind.WindSpeed, tafRecord.Wind.WindSpeed);
            Assert.Equal(wind.WindGust, tafRecord.Wind.WindGust);

            Assert.Equal("6000м", tafRecord.Visibility);

            Assert.Equal("снег (слаб. интенсивн.)", tafRecord.MainConditions);

            Assert.Equal("сплошная, нижн. гран. 180м", string.Join(", ", tafRecord.MainSkies));

            Temperature max = new()
            {
                DateTime = new DateTime(2022, 11, 21, 12, 0, 0, DateTimeKind.Utc),
                TemperatureValue = 4,
                UnderZero = true
            };
            Assert.Equal(max.DateTime, tafRecord.MaxTemperature.DateTime);
            Assert.Equal(max.TemperatureValue, tafRecord.MaxTemperature.TemperatureValue);
            Assert.Equal(max.UnderZero, tafRecord.MaxTemperature.UnderZero);

            Temperature min = new()
            {
                DateTime = new DateTime(2022, 11, 21, 3, 0, 0, DateTimeKind.Utc),
                TemperatureValue = 8,
                UnderZero = true
            };
            Assert.Equal(min.DateTime, tafRecord.MinTemperature.DateTime);
            Assert.Equal(min.TemperatureValue, tafRecord.MinTemperature.TemperatureValue);
            Assert.Equal(min.UnderZero, tafRecord.MinTemperature.UnderZero);


            Assert.Equal(2, tafRecord.Trends.Count);

            var tempo = tafRecord.Trends[0];
            Assert.Equal("TEMPO 2103/2110 1000 FZDZ BR OVC003", tempo.TafText);

            Assert.Equal(new DateTime(2022, 11, 21, 3, 0, 0, DateTimeKind.Utc), tempo.TimeFrom);
            Assert.Equal(new DateTime(2022, 11, 21, 10, 0, 0, DateTimeKind.Utc), tempo.TimeTo);

            Assert.Equal("временами видимость 1000м, замерзающая морось (умер. интенсивн.), дымка, облачность сплошная нижн. гран. 90м", tempo.Info);

            Assert.Equal("1000м", tempo.Visibility);
            Assert.Equal("сплошная, нижн. гран. 90м", string.Join(", ", tempo.Skies));

            var becmg = tafRecord.Trends[1];
            Assert.Equal("BECMG 2110/2112 BKN013", becmg.TafText);

            Assert.Equal(new DateTime(2022, 11, 21, 10, 0, 0, DateTimeKind.Utc), becmg.TimeFrom);
            Assert.Equal(new DateTime(2022, 11, 21, 12, 0, 0, DateTimeKind.Utc), becmg.TimeTo);

            Assert.Equal("облачность значительная нижн. гран. 390м", becmg.Info);

            Assert.Empty(becmg.Conditions);

        }
        [Fact]
        public void TafTest3()
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load("../../../tafExamples/3.xml");
            var xml = xDoc.OuterXml;

            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var tafs = AWC.ParseTafData(xml);

            Assert.Single(tafs);
            var m = tafs.First();

            var tafRecord = new MeteoRecordFactory(Language.RUS).CreateFrom(m);
            Assert.Equal("TAF USSS 201051Z 2012/2112 27003G10MPS 9999 SCT020", tafRecord.MainRaw);
            Assert.Equal("USSS", tafRecord.StationId);

            Assert.Equal(new DateTime(2022, 11, 20, 10, 51, 0, DateTimeKind.Utc), tafRecord.IssueTime);

            Assert.Equal(new DateTime(2022, 11, 20, 12, 0, 0, DateTimeKind.Utc), tafRecord.Start);
            Assert.Equal(new DateTime(2022, 11, 21, 12, 0, 0, DateTimeKind.Utc), tafRecord.End);

            WindRecord wind = new()
            {
                WindSpeed = 3d,
                WindGust = 10d,
                WindDirection = "запад"
            };
            //Assert.Equal(wind, tafRecord.Wind);
            Assert.Equal(wind.WindDirection, tafRecord.Wind.WindDirection);
            Assert.Equal(wind.WindSpeed, tafRecord.Wind.WindSpeed);
            Assert.Equal(wind.WindGust, tafRecord.Wind.WindGust);

            Assert.Equal("более 10000м", tafRecord.Visibility);

            Assert.Equal("разбросанная, нижн. гран. 600м", string.Join(", ", tafRecord.MainSkies));

            Assert.Equal(2, tafRecord.Trends.Count);

            var becmg1 = tafRecord.Trends[0];
            Assert.Equal("BECMG 2015/2017 6000", becmg1.TafText);

            Assert.Equal(new DateTime(2022, 11, 20, 15, 0, 0, DateTimeKind.Utc), becmg1.TimeFrom);
            Assert.Equal(new DateTime(2022, 11, 20, 17, 0, 0, DateTimeKind.Utc), becmg1.TimeTo);

            Assert.Equal("видимость 6000м", becmg1.Info);

            var becmg2 = tafRecord.Trends[1];
            Assert.Equal("BECMG 2106/2109 9999", becmg2.TafText);

            Assert.Equal(new DateTime(2022, 11, 21, 6, 0, 0, DateTimeKind.Utc), becmg2.TimeFrom);
            Assert.Equal(new DateTime(2022, 11, 21, 9, 0, 0, DateTimeKind.Utc), becmg2.TimeTo);

            Assert.Equal("видимость более 10000м", becmg2.Info);
        }
        [Fact]
        public void TafTest4()
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load("../../../tafExamples/4.xml");
            var xml = xDoc.OuterXml;

            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var tafs = AWC.ParseTafData(xml);

            Assert.Single(tafs);
            var m = tafs.First();

            var tafRecord = new MeteoRecordFactory(Language.RUS).CreateFrom(m);
            Assert.Equal("TAF USSS 220753Z 2209/2309 35003G10MPS 9999 -SHSN SCT016CB", tafRecord.MainRaw);
            Assert.Equal("USSS", tafRecord.StationId);

            Assert.Equal(new DateTime(2022, 11, 22, 7, 53, 0, DateTimeKind.Utc), tafRecord.IssueTime);

            Assert.Equal(new DateTime(2022, 11, 22, 9, 0, 0, DateTimeKind.Utc), tafRecord.Start);
            Assert.Equal(new DateTime(2022, 11, 23, 9, 0, 0, DateTimeKind.Utc), tafRecord.End);

            WindRecord wind = new()
            {
                WindSpeed = 3d,
                WindGust = 10d,
                WindDirection = "север"
            };
            //Assert.Equal(wind, tafRecord.Wind);
            Assert.Equal(wind.WindDirection, tafRecord.Wind.WindDirection);
            Assert.Equal(wind.WindSpeed, tafRecord.Wind.WindSpeed);
            Assert.Equal(wind.WindGust, tafRecord.Wind.WindGust);

            Assert.Equal("более 10000м", tafRecord.Visibility);

            Assert.Equal("ливневой снег (слаб. интенсивн.)", tafRecord.MainConditions);

            Assert.Equal("разбросанная, кучево-дождевые облака, нижн. гран. 480м", string.Join(", ", tafRecord.MainSkies));

            Assert.Equal(4, tafRecord.Trends.Count);

            var fm = tafRecord.Trends[0];
            Assert.Equal("FM221200 02003G08MPS 6000 SCT016", fm.TafText);

            Assert.Equal(new DateTime(2022, 11, 22, 12, 0, 0, DateTimeKind.Utc), fm.TimeFrom);

            WindRecord fmWind = new()
            {
                WindSpeed = 3d,
                WindGust = 8d,
                WindDirection = "север"
            };
            //Assert.Equal(wind, tafRecord.Wind);
            Assert.Equal(fmWind.WindDirection, fm.Wind.WindDirection);
            Assert.Equal(fmWind.WindSpeed, fm.Wind.WindSpeed);
            Assert.Equal(fmWind.WindGust, fm.Wind.WindGust);

            Assert.Equal("ветер север 3м/с с порывами 8м/с, видимость 6000м, облачность разбросанная нижн. гран. 480м", fm.Info);

            var tempo = tafRecord.Trends[1];
            Assert.Equal("TEMPO 2215/2303 0300 FZFG", tempo.TafText);

            Assert.Equal(new DateTime(2022, 11, 22, 15, 0, 0, DateTimeKind.Utc), tempo.TimeFrom);
            Assert.Equal(new DateTime(2022, 11, 23, 3, 0, 0, DateTimeKind.Utc), tempo.TimeTo);

            Assert.Equal("временами видимость 300м, замерзающий туман", tempo.Info);

            var becmg1 = tafRecord.Trends[2];
            Assert.Equal("BECMG 2215/2217 12003MPS", becmg1.TafText);

            Assert.Equal(new DateTime(2022, 11, 22, 15, 0, 0, DateTimeKind.Utc), becmg1.TimeFrom);
            Assert.Equal(new DateTime(2022, 11, 22, 17, 0, 0, DateTimeKind.Utc), becmg1.TimeTo);

            WindRecord becmg1Wind = new()
            {
                WindSpeed = 3d,
                WindGust = null,
                WindDirection = "юго-восток"
            };
            //Assert.Equal(wind, tafRecord.Wind);
            Assert.Equal(becmg1Wind.WindDirection, becmg1.Wind.WindDirection);
            Assert.Equal(becmg1Wind.WindSpeed, becmg1.Wind.WindSpeed);
            Assert.Equal(becmg1Wind.WindGust, becmg1.Wind.WindGust);

            Assert.Equal("ветер юго-восток 3м/с", becmg1.Info);

            var becmg2 = tafRecord.Trends[3];
            Assert.Equal("BECMG 2304/2307 14003G10MPS 9999", becmg2.TafText);

            Assert.Equal(new DateTime(2022, 11, 23, 4, 0, 0, DateTimeKind.Utc), becmg2.TimeFrom);
            Assert.Equal(new DateTime(2022, 11, 23, 7, 0, 0, DateTimeKind.Utc), becmg2.TimeTo);

            WindRecord becmg2Wind = new()
            {
                WindSpeed = 3d,
                WindGust = 10d,
                WindDirection = "юго-восток"
            };
            //Assert.Equal(wind, tafRecord.Wind);
            Assert.Equal(becmg2Wind.WindDirection, becmg2.Wind.WindDirection);
            Assert.Equal(becmg2Wind.WindSpeed, becmg2.Wind.WindSpeed);
            Assert.Equal(becmg2Wind.WindGust, becmg2.Wind.WindGust);

            Assert.Equal("ветер юго-восток 3м/с с порывами 10м/с, видимость более 10000м", becmg2.Info);
        }
        [Fact]
        public void TafTest8()
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load("../../../tafExamples/8.xml");
            var xml = xDoc.OuterXml;

            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var tafs = AWC.ParseTafData(xml);

            Assert.Single(tafs);
            var m = tafs.First();

            var tafRecord = new MeteoRecordFactory(Language.RUS).CreateFrom(m);
            Assert.Equal("TAF USSS 110754Z 1109/1209 13003G13MPS 6000 -SHRA BKN016CB", tafRecord.MainRaw);
            Assert.Equal("USSS", tafRecord.StationId);

            Assert.Equal(new DateTime(2022, 5, 11, 7, 54, 0, DateTimeKind.Utc), tafRecord.IssueTime);

            Assert.Equal(new DateTime(2022, 5, 11, 9, 0, 0, DateTimeKind.Utc), tafRecord.Start);
            Assert.Equal(new DateTime(2022, 5, 12, 9, 0, 0, DateTimeKind.Utc), tafRecord.End);

            WindRecord wind = new()
            {
                WindSpeed = 3d,
                WindGust = 13d,
                WindDirection = "юго-восток"
            };
            //Assert.Equal(wind, tafRecord.Wind);
            Assert.Equal(wind.WindDirection, tafRecord.Wind.WindDirection);
            Assert.Equal(wind.WindSpeed, tafRecord.Wind.WindSpeed);
            Assert.Equal(wind.WindGust, tafRecord.Wind.WindGust);

            Assert.Equal("6000м", tafRecord.Visibility);

            Assert.Equal("ливневой дождь (слаб. интенсивн.)", tafRecord.MainConditions);

            Assert.Equal("значительная, кучево-дождевые облака, нижн. гран. 480м", string.Join(", ", tafRecord.MainSkies));

            Assert.Equal(5, tafRecord.Trends.Count);

            var tempo1 = tafRecord.Trends[0];
            Assert.Equal("TEMPO 1109/1112 VV005", tempo1.TafText);

            Assert.Equal(new DateTime(2022, 5, 11, 9, 0, 0, DateTimeKind.Utc), tempo1.TimeFrom);
            Assert.Equal(new DateTime(2022, 5, 11, 12, 0, 0, DateTimeKind.Utc), tempo1.TimeTo);

            Assert.Equal(150, tempo1.VerticalVisibility);
            Assert.Equal("временами вертикальная видимость 150м", tempo1.Info);

            var becmg1 = tafRecord.Trends[1];
            Assert.Equal("BECMG 1112/1113 25005G15MPS 9999 -SHRA", becmg1.TafText);

            Assert.Equal(new DateTime(2022, 5, 11, 12, 0, 0, DateTimeKind.Utc), becmg1.TimeFrom);
            Assert.Equal(new DateTime(2022, 5, 11, 13, 0, 0, DateTimeKind.Utc), becmg1.TimeTo);

            WindRecord becmg1Wind = new()
            {
                WindSpeed = 5d,
                WindGust = 15d,
                WindDirection = "запад"
            };
            //Assert.Equal(wind, tafRecord.Wind);
            Assert.Equal(becmg1Wind.WindDirection, becmg1.Wind.WindDirection);
            Assert.Equal(becmg1Wind.WindSpeed, becmg1.Wind.WindSpeed);
            Assert.Equal(becmg1Wind.WindGust, becmg1.Wind.WindGust);

            Assert.Equal("ветер запад 5м/с с порывами 15м/с, видимость более 10000м, ливневой дождь (слаб. интенсивн.)", becmg1.Info);


            var fm = tafRecord.Trends[2];
            Assert.Equal("FM111800 21003G08MPS 6000 BKN016", fm.TafText);

            Assert.Equal(new DateTime(2022, 5, 11, 18, 0, 0, DateTimeKind.Utc), fm.TimeFrom);

            WindRecord fmWind = new()
            {
                WindSpeed = 3d,
                WindGust = 8d,
                WindDirection = "юго-запад"
            };

            Assert.Equal(fmWind.WindDirection, fm.Wind.WindDirection);
            Assert.Equal(fmWind.WindSpeed, fm.Wind.WindSpeed);
            Assert.Equal(fmWind.WindGust, fm.Wind.WindGust);

            Assert.Equal("ветер юго-запад 3м/с с порывами 8м/с, видимость 6000м, облачность значительная нижн. гран. 480м", fm.Info);


            var tempo2 = tafRecord.Trends[3];
            Assert.Equal("TEMPO 1118/1203 0500 FG", tempo2.TafText);

            Assert.Equal(new DateTime(2022, 5, 11, 18, 0, 0, DateTimeKind.Utc), tempo2.TimeFrom);
            Assert.Equal(new DateTime(2022, 5, 12, 3, 0, 0, DateTimeKind.Utc), tempo2.TimeTo);

            Assert.Equal("временами видимость 500м, туман", tempo2.Info);


            var becmg2 = tafRecord.Trends[4];
            Assert.Equal("BECMG 1204/1206 17003G12MPS 9999", becmg2.TafText);

            Assert.Equal(new DateTime(2022, 5, 12, 4, 0, 0, DateTimeKind.Utc), becmg2.TimeFrom);
            Assert.Equal(new DateTime(2022, 5, 12, 6, 0, 0, DateTimeKind.Utc), becmg2.TimeTo);

            WindRecord becmg2Wind = new()
            {
                WindSpeed = 3d,
                WindGust = 12d,
                WindDirection = "юг"
            };
            //Assert.Equal(wind, tafRecord.Wind);
            Assert.Equal(becmg2Wind.WindDirection, becmg2.Wind.WindDirection);
            Assert.Equal(becmg2Wind.WindSpeed, becmg2.Wind.WindSpeed);
            Assert.Equal(becmg2Wind.WindGust, becmg2.Wind.WindGust);

            Assert.Equal("ветер юг 3м/с с порывами 12м/с, видимость более 10000м", becmg2.Info);
        }
        [Fact]
        public void TafTest9()
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load("../../../tafExamples/9.xml");
            var xml = xDoc.OuterXml;

            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var tafs = AWC.ParseTafData(xml);

            Assert.Single(tafs);
            var m = tafs.First();

            var tafRecord = new MeteoRecordFactory(Language.RUS).CreateFrom(m);
            Assert.Equal("TAF UWGG 170750Z 1709/1809 24003G09MPS 9999 SCT040CB TX24/1712Z TN12/1801Z", tafRecord.MainRaw);
            Assert.Equal("UWGG", tafRecord.StationId);

            Assert.Equal(new DateTime(2022, 6, 17, 7, 50, 0, DateTimeKind.Utc), tafRecord.IssueTime);

            Assert.Equal(new DateTime(2022, 6, 17, 9, 0, 0, DateTimeKind.Utc), tafRecord.Start);
            Assert.Equal(new DateTime(2022, 6, 18, 9, 0, 0, DateTimeKind.Utc), tafRecord.End);

            WindRecord wind = new()
            {
                WindSpeed = 3d,
                WindGust = 9d,
                WindDirection = "юго-запад"
            };
            //Assert.Equal(wind, tafRecord.Wind);
            Assert.Equal(wind.WindDirection, tafRecord.Wind.WindDirection);
            Assert.Equal(wind.WindSpeed, tafRecord.Wind.WindSpeed);
            Assert.Equal(wind.WindGust, tafRecord.Wind.WindGust);

            Assert.Equal("более 10000м", tafRecord.Visibility);

            Assert.Equal("разбросанная, кучево-дождевые облака, нижн. гран. 1210м", string.Join(", ", tafRecord.MainSkies));

            Temperature max = new()
            {
                DateTime = new DateTime(2022, 6, 17, 12, 0, 0, DateTimeKind.Utc),
                TemperatureValue = 24,
                UnderZero = false
            };
            Assert.Equal(max.DateTime, tafRecord.MaxTemperature.DateTime);
            Assert.Equal(max.TemperatureValue, tafRecord.MaxTemperature.TemperatureValue);
            Assert.Equal(max.UnderZero, tafRecord.MaxTemperature.UnderZero);

            Temperature min = new()
            {
                DateTime = new DateTime(2022, 6, 18, 1, 0, 0, DateTimeKind.Utc),
                TemperatureValue = 12,
                UnderZero = false
            };
            Assert.Equal(min.DateTime, tafRecord.MinTemperature.DateTime);
            Assert.Equal(min.TemperatureValue, tafRecord.MinTemperature.TemperatureValue);
            Assert.Equal(min.UnderZero, tafRecord.MinTemperature.UnderZero);

            Assert.Equal(2, tafRecord.Trends.Count);

            var tempo = tafRecord.Trends[0];
            Assert.Equal("TEMPO 1709/1717 26008G14MPS", tempo.TafText);

            Assert.Equal(new DateTime(2022, 6, 17, 9, 0, 0, DateTimeKind.Utc), tempo.TimeFrom);
            Assert.Equal(new DateTime(2022, 6, 17, 17, 0, 0, DateTimeKind.Utc), tempo.TimeTo);

            WindRecord tempoWind = new()
            {
                WindSpeed = 8d,
                WindGust = 14d,
                WindDirection = "запад"
            };
            //Assert.Equal(wind, tafRecord.Wind);
            Assert.Equal(tempoWind.WindDirection, tempo.Wind.WindDirection);
            Assert.Equal(tempoWind.WindSpeed, tempo.Wind.WindSpeed);
            Assert.Equal(tempoWind.WindGust, tempo.Wind.WindGust);

            Assert.Equal("временами ветер запад 8м/с с порывами 14м/с", tempo.Info);

            var tempo2 = tafRecord.Trends[1];
            Assert.Equal("PROB40 TEMPO 1709/1717 -TSRA", tempo2.TafText);

            Assert.Equal(new DateTime(2022, 6, 17, 9, 0, 0, DateTimeKind.Utc), tempo2.TimeFrom);
            Assert.Equal(new DateTime(2022, 6, 17, 17, 0, 0, DateTimeKind.Utc), tempo2.TimeTo);

            Assert.Equal("с вероятностью 40% временами гроза с дождём (слаб. интенсивн.)", tempo2.Info);
        }
        [Fact]
        public void TafTest10()
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load("../../../tafExamples/16.xml");
            var xml = xDoc.OuterXml;

            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var tafs = AWC.ParseTafData(xml);
            var m = tafs.First();

            var tafRecord = new MeteoRecordFactory(Language.RUS).CreateFrom(m);
            Assert.NotNull(tafRecord);
        }
        [Fact]
        public void TafProbTest1()
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load("../../../tafExamples/prob/1.xml");
            var xml = xDoc.OuterXml;

            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var tafs = AWC.ParseTafData(xml);

            Assert.Single(tafs);
            var m = tafs.First();

            var tafRecord = new MeteoRecordFactory(Language.RUS).CreateFrom(m);
            Assert.Equal("TAF URRP 090455Z 0906/1006 12008G14MPS 6000 FEW004 SCT016CB", tafRecord.MainRaw);
            Assert.Equal("URRP", tafRecord.StationId);

            Assert.Equal(new DateTime(2022, 12, 9, 4, 55, 0, DateTimeKind.Utc), tafRecord.IssueTime);

            Assert.Equal(new DateTime(2022, 12, 9, 6, 0, 0, DateTimeKind.Utc), tafRecord.Start);
            Assert.Equal(new DateTime(2022, 12, 10, 6, 0, 0, DateTimeKind.Utc), tafRecord.End);

            WindRecord wind = new()
            {
                WindSpeed = 8d,
                WindGust = 14d,
                WindDirection = "юго-восток"
            };
            //Assert.Equal(wind, tafRecord.Wind);
            Assert.Equal(wind.WindDirection, tafRecord.Wind.WindDirection);
            Assert.Equal(wind.WindSpeed, tafRecord.Wind.WindSpeed);
            Assert.Equal(wind.WindGust, tafRecord.Wind.WindGust);

            Assert.Equal("6000м", tafRecord.Visibility);

            Assert.Equal("незначительная, нижн. гран. 120м, разбросанная, кучево-дождевые облака, нижн. гран. 480м", string.Join(", ", tafRecord.MainSkies));


            Assert.Equal(2, tafRecord.Trends.Count);

            var tempo = tafRecord.Trends[0];
            Assert.Equal("TEMPO 0906/1006 14010G16MPS -SHRASN SCT003 BKN020CB", tempo.TafText);

            Assert.Equal(new DateTime(2022, 12, 9, 6, 0, 0, DateTimeKind.Utc), tempo.TimeFrom);
            Assert.Equal(new DateTime(2022, 12, 10, 6, 0, 0, DateTimeKind.Utc), tempo.TimeTo);

            WindRecord tempoWind = new()
            {
                WindSpeed = 10d,
                WindGust = 16d,
                WindDirection = "юго-восток"
            };
            //Assert.Equal(wind, tafRecord.Wind);
            Assert.Equal(tempoWind.WindDirection, tempo.Wind.WindDirection);
            Assert.Equal(tempoWind.WindSpeed, tempo.Wind.WindSpeed);
            Assert.Equal(tempoWind.WindGust, tempo.Wind.WindGust);

            Assert.Equal("временами ветер юго-восток 10м/с с порывами 16м/с, ливневой снег с дождем (слаб. интенсивн.), облачность разбросанная нижн. гран. 90м значительная кучево-дождевые облака нижн. гран. 600м", tempo.Info);

            var tempo2 = tafRecord.Trends[1];
            Assert.Equal("PROB40 TEMPO 0906/1006 -FZRA BKN002 OVC016 BKN030CB", tempo2.TafText);

            Assert.Equal(new DateTime(2022, 12, 9, 6, 0, 0, DateTimeKind.Utc), tempo2.TimeFrom);
            Assert.Equal(new DateTime(2022, 12, 10, 6, 0, 0, DateTimeKind.Utc), tempo2.TimeTo);

            Assert.Equal("с вероятностью 40% временами замерзающий дождь (слаб. интенсивн.), облачность значительная нижн. гран. 60м сплошная нижн. гран. 480м значительная кучево-дождевые облака нижн. гран. 910м", tempo2.Info);
        }
        [Fact]
        public void TafProbTest2()
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load("../../../tafExamples/prob/2.xml");
            var xml = xDoc.OuterXml;

            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var tafs = AWC.ParseTafData(xml);

            Assert.Single(tafs);
            var m = tafs.First();

            var tafRecord = new MeteoRecordFactory(Language.RUS).CreateFrom(m);
            Assert.Equal("TAF UWWW 300751Z 3009/0109 36003MPS 6000 -SN BKN007 TXM09/3011Z TNM12/0103Z", tafRecord.MainRaw);
            Assert.Equal("UWWW", tafRecord.StationId);

            Assert.Equal(new DateTime(2022, 11, 30, 7, 51, 0, DateTimeKind.Utc), tafRecord.IssueTime);

            Assert.Equal(new DateTime(2022, 11, 30, 9, 0, 0, DateTimeKind.Utc), tafRecord.Start);
            Assert.Equal(new DateTime(2022, 12, 1, 9, 0, 0, DateTimeKind.Utc), tafRecord.End);

            WindRecord wind = new()
            {
                WindSpeed = 3d,
                WindDirection = "север"
            };
            //Assert.Equal(wind, tafRecord.Wind);
            Assert.Equal(wind.WindDirection, tafRecord.Wind.WindDirection);
            Assert.Equal(wind.WindSpeed, tafRecord.Wind.WindSpeed);
            Assert.Equal(wind.WindGust, tafRecord.Wind.WindGust);

            Assert.Equal("6000м", tafRecord.Visibility);
            Assert.Equal("снег (слаб. интенсивн.)", tafRecord.MainConditions);

            Assert.Equal("значительная, нижн. гран. 210м", string.Join(", ", tafRecord.MainSkies));

            Temperature max = new()
            {
                DateTime = new DateTime(2022, 11, 30, 11, 0, 0, DateTimeKind.Utc),
                TemperatureValue = 9,
                UnderZero = true
            };
            Assert.Equal(max.DateTime, tafRecord.MaxTemperature.DateTime);
            Assert.Equal(max.TemperatureValue, tafRecord.MaxTemperature.TemperatureValue);
            Assert.Equal(max.UnderZero, tafRecord.MaxTemperature.UnderZero);

            Temperature min = new()
            {
                DateTime = new DateTime(2022, 12, 1, 3, 0, 0, DateTimeKind.Utc),
                TemperatureValue = 12,
                UnderZero = true
            };
            Assert.Equal(min.DateTime, tafRecord.MinTemperature.DateTime);
            Assert.Equal(min.TemperatureValue, tafRecord.MinTemperature.TemperatureValue);
            Assert.Equal(min.UnderZero, tafRecord.MinTemperature.UnderZero);

            Assert.Equal(2, tafRecord.Trends.Count);

            var tempo = tafRecord.Trends[0];
            Assert.Equal("TEMPO 3009/3015 1000 SN BR OVC003", tempo.TafText);

            Assert.Equal(new DateTime(2022, 11, 30, 9, 0, 0, DateTimeKind.Utc), tempo.TimeFrom);
            Assert.Equal(new DateTime(2022, 11, 30, 15, 0, 0, DateTimeKind.Utc), tempo.TimeTo);

            Assert.Equal("временами видимость 1000м, снег (умер. интенсивн.), дымка, облачность сплошная нижн. гран. 90м", tempo.Info);

            var prob = tafRecord.Trends[1];
            Assert.Equal("PROB40 3015/0107 0300 FZFG VV002", prob.TafText);

            Assert.Equal(new DateTime(2022, 11, 30, 15, 0, 0, DateTimeKind.Utc), prob.TimeFrom);
            Assert.Equal(new DateTime(2022, 12, 1, 7, 0, 0, DateTimeKind.Utc), prob.TimeTo);

            Assert.Equal("с вероятностью 40% видимость 300м, замерзающий туман, вертикальная видимость 60м", prob.Info);
            Assert.Equal(60, prob.VerticalVisibility);
        }
        [Fact]
        public void TafProbTest5()
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load("../../../tafExamples/prob/5.xml");
            var xml = xDoc.OuterXml;

            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var tafs = AWC.ParseTafData(xml);

            Assert.Single(tafs);
            var m = tafs.First();

            var tafRecord = new MeteoRecordFactory(Language.RUS).CreateFrom(m);

            Assert.Equal("TAF USSS 231050Z 2312/2412 16003MPS 9999 OVC011 TXM02/2411Z TNM07/2403Z", tafRecord.MainRaw);
            Assert.Equal("USSS", tafRecord.StationId);

            Assert.Equal(new DateTime(2023, 1, 23, 10, 50, 0, DateTimeKind.Utc), tafRecord.IssueTime);

            Assert.Equal(new DateTime(2023, 1, 23, 12, 0, 0, DateTimeKind.Utc), tafRecord.Start);
            Assert.Equal(new DateTime(2023, 1, 24, 12, 0, 0, DateTimeKind.Utc), tafRecord.End);

            WindRecord wind = new()
            {
                WindSpeed = 3d,
                WindDirection = "юг"
            };
            //Assert.Equal(wind, tafRecord.Wind);
            Assert.Equal(wind.WindDirection, tafRecord.Wind.WindDirection);
            Assert.Equal(wind.WindSpeed, tafRecord.Wind.WindSpeed);
            Assert.Equal(wind.WindGust, tafRecord.Wind.WindGust);

            Assert.Equal("более 10000м", tafRecord.Visibility);

            Assert.Equal("сплошная, нижн. гран. 330м", string.Join(", ", tafRecord.MainSkies));

            Temperature max = new()
            {
                DateTime = new DateTime(2023, 1, 24, 11, 0, 0, DateTimeKind.Utc),
                TemperatureValue = 2,
                UnderZero = true
            };
            Assert.Equal(max.DateTime, tafRecord.MaxTemperature.DateTime);
            Assert.Equal(max.TemperatureValue, tafRecord.MaxTemperature.TemperatureValue);
            Assert.Equal(max.UnderZero, tafRecord.MaxTemperature.UnderZero);

            Temperature min = new()
            {
                DateTime = new DateTime(2023, 1, 24, 3, 0, 0, DateTimeKind.Utc),
                TemperatureValue = 7,
                UnderZero = true
            };
            Assert.Equal(min.DateTime, tafRecord.MinTemperature.DateTime);
            Assert.Equal(min.TemperatureValue, tafRecord.MinTemperature.TemperatureValue);
            Assert.Equal(min.UnderZero, tafRecord.MinTemperature.UnderZero);

            Assert.Equal(2, tafRecord.Trends.Count);

            var tempo = tafRecord.Trends[0];
            Assert.Equal("TEMPO 2312/2318 OVC004", tempo.TafText);

            Assert.Equal(new DateTime(2023, 1, 23, 12, 0, 0, DateTimeKind.Utc), tempo.TimeFrom);
            Assert.Equal(new DateTime(2023, 1, 23, 18, 0, 0, DateTimeKind.Utc), tempo.TimeTo);

            Assert.Equal("сплошная, нижн. гран. 120м", string.Join(", ", tempo.Skies));
            Assert.Equal("временами облачность сплошная нижн. гран. 120м", tempo.Info);

            var prob = tafRecord.Trends[1];
            Assert.Equal("PROB40 TEMPO 2318/2406 0300 FZFG OVC002", prob.TafText);

            Assert.Equal(new DateTime(2023, 1, 23, 18, 0, 0, DateTimeKind.Utc), prob.TimeFrom);
            Assert.Equal(new DateTime(2023, 1, 24, 6, 0, 0, DateTimeKind.Utc), prob.TimeTo);

            Assert.Equal("с вероятностью 40% временами видимость 300м, замерзающий туман, облачность сплошная нижн. гран. 60м", prob.Info);
        }
        [Fact]
        public void TafMixedTest1()
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load("../../../tafExamples/mixedForecast/1.xml");
            var xml = xDoc.OuterXml;

            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var tafs = AWC.ParseTafData(xml);

            Assert.Single(tafs);
            var m = tafs.First();

            var tafRecord = new MeteoRecordFactory(Language.RUS).CreateFrom(m);

            Assert.Equal("TAF UHPP 240731Z 2409/2515 35003G10MPS 9999 SCT030 TXM02/2509Z TNM20/2411Z", tafRecord.MainRaw);
            Assert.Equal("UHPP", tafRecord.StationId);

            Assert.Equal(new DateTime(2023, 1, 24, 7, 31, 0, DateTimeKind.Utc), tafRecord.IssueTime);

            Assert.Equal(new DateTime(2023, 1, 24, 9, 0, 0, DateTimeKind.Utc), tafRecord.Start);
            Assert.Equal(new DateTime(2023, 1, 25, 15, 0, 0, DateTimeKind.Utc), tafRecord.End);

            WindRecord wind = new()
            {
                WindSpeed = 3d,
                WindGust = 10d,
                WindDirection = "север"
            };
            //Assert.Equal(wind, tafRecord.Wind);
            Assert.Equal(wind.WindDirection, tafRecord.Wind.WindDirection);
            Assert.Equal(wind.WindSpeed, tafRecord.Wind.WindSpeed);
            Assert.Equal(wind.WindGust, tafRecord.Wind.WindGust);

            Assert.Equal("более 10000м", tafRecord.Visibility);

            Assert.Equal("разбросанная, нижн. гран. 910м", string.Join(", ", tafRecord.MainSkies));

            Temperature max = new()
            {
                DateTime = new DateTime(2023, 1, 25, 9, 0, 0, DateTimeKind.Utc),
                TemperatureValue = 2,
                UnderZero = true
            };
            Assert.Equal(max.DateTime, tafRecord.MaxTemperature.DateTime);
            Assert.Equal(max.TemperatureValue, tafRecord.MaxTemperature.TemperatureValue);
            Assert.Equal(max.UnderZero, tafRecord.MaxTemperature.UnderZero);

            Temperature min = new()
            {
                DateTime = new DateTime(2023, 1, 24, 11, 0, 0, DateTimeKind.Utc),
                TemperatureValue = 20,
                UnderZero = true
            };
            Assert.Equal(min.DateTime, tafRecord.MinTemperature.DateTime);
            Assert.Equal(min.TemperatureValue, tafRecord.MinTemperature.TemperatureValue);
            Assert.Equal(min.UnderZero, tafRecord.MinTemperature.UnderZero);

            Assert.Equal(4, tafRecord.Trends.Count);

            var becmg = tafRecord.Trends[0];
            Assert.Equal("BECMG 2420/2422 06008MPS BKN012CB", becmg.TafText);

            Assert.Equal(new DateTime(2023, 1, 24, 20, 0, 0, DateTimeKind.Utc), becmg.TimeFrom);
            Assert.Equal(new DateTime(2023, 1, 24, 22, 0, 0, DateTimeKind.Utc), becmg.TimeTo);

            WindRecord becmgWind = new()
            {
                WindSpeed = 8d,
                WindDirection = "северо-восток"
            };

            Assert.Equal(becmgWind.WindDirection, becmg.Wind.WindDirection);
            Assert.Equal(becmgWind.WindSpeed, becmg.Wind.WindSpeed);
            Assert.Equal(becmgWind.WindGust, becmg.Wind.WindGust);

            Assert.Equal("ветер северо-восток 8м/с, облачность значительная кучево-дождевые облака нижн. гран. 360м", becmg.Info);

            var fm = tafRecord.Trends[1];
            Assert.Equal("FM250300 08013G25MPS 2500 -SHSN BLSN BKN007 OVC015CB", fm.TafText);

            Assert.Equal(new DateTime(2023, 1, 25, 3, 0, 0, DateTimeKind.Utc), fm.TimeFrom);

            Assert.Equal("ветер восток 13м/с с порывами 25м/с, видимость 2500м, ливневой снег (слаб. интенсивн.), низовая метель, облачность значительная нижн. гран. 210м сплошная кучево-дождевые облака нижн. гран. 450м", fm.Info);

            var tempo = tafRecord.Trends[2];
            Assert.Equal("TEMPO 2503/2506 0500 +SHSN BLSN VV002", tempo.TafText);
            Assert.Equal(new DateTime(2023, 1, 25, 3, 0, 0, DateTimeKind.Utc), tempo.TimeFrom);
            Assert.Equal(new DateTime(2023, 1, 25, 6, 0, 0, DateTimeKind.Utc), tempo.TimeTo);
            Assert.Equal("временами видимость 500м, ливневой снег (сильн. интенсивн.), низовая метель, вертикальная видимость 60м", tempo.Info);

            var fm2 = tafRecord.Trends[3];
            Assert.Equal("FM250800 05018G33MPS 0700 +SHSN BLSN VV002", fm2.TafText);
            Assert.Equal(new DateTime(2023, 1, 25, 8, 0, 0, DateTimeKind.Utc), fm2.TimeFrom);
            Assert.Equal("ветер северо-восток 18м/с с порывами 33м/с, видимость 700м, ливневой снег (сильн. интенсивн.), низовая метель, вертикальная видимость 60м", fm2.Info);

        }
        [Fact]
        public void TafCavokTest1()
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load("../../../tafExamples/CAVOK/1.xml");
            var xml = xDoc.OuterXml;

            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var tafs = AWC.ParseTafData(xml);

            Assert.Single(tafs);
            var m = tafs.First();

            var tafRecord = new MeteoRecordFactory(Language.RUS).CreateFrom(m);
            Assert.Equal("TAF UHBB 200740Z 2009/2109 33003G08MPS CAVOK", tafRecord.MainRaw);
            Assert.Equal("UHBB", tafRecord.StationId);

            Assert.Equal(new DateTime(2023, 2, 20, 7, 40, 0, DateTimeKind.Utc), tafRecord.IssueTime);

            Assert.Equal(new DateTime(2023, 2, 20, 9, 0, 0, DateTimeKind.Utc), tafRecord.Start);
            Assert.Equal(new DateTime(2023, 2, 21, 9, 0, 0, DateTimeKind.Utc), tafRecord.End);

            WindRecord wind = new()
            {
                WindSpeed = 3d,
                WindGust = 8d,
                WindDirection = "северо-запад"
            };
            //Assert.Equal(wind, tafRecord.Wind);
            Assert.Equal(wind.WindDirection, tafRecord.Wind.WindDirection);
            Assert.Equal(wind.WindSpeed, tafRecord.Wind.WindSpeed);
            Assert.Equal(wind.WindGust, tafRecord.Wind.WindGust);

            Assert.Equal("более 10000м", tafRecord.Visibility);
            Assert.Equal("нет значимой для полетов облачности", string.Join(", ", tafRecord.MainSkies));

            var becmg = tafRecord.Trends[0];
            Assert.Equal("BECMG 2023/2101 28003G08MPS", becmg.TafText);
            Assert.Equal(new DateTime(2023, 2, 20, 23, 0, 0, DateTimeKind.Utc), becmg.TimeFrom);
            Assert.Equal(new DateTime(2023, 2, 21, 1, 0, 0, DateTimeKind.Utc), becmg.TimeTo);
            Assert.Equal("ветер запад 3м/с с порывами 8м/с", becmg.Info);

        }
        [Fact]
        public void TafVVTest1()
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load("../../../tafExamples/VV/1.xml");
            var xml = xDoc.OuterXml;

            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var tafs = AWC.ParseTafData(xml);

            Assert.Single(tafs);
            var m = tafs.First();

            var tafRecord = new MeteoRecordFactory(Language.RUS).CreateFrom(m);
            Assert.Equal("TAF UHPP 240731Z 2409/2515 35003G10MPS 9999 SCT030 TXM02/2509Z TNM20/2411Z", tafRecord.MainRaw);
            Assert.Equal("UHPP", tafRecord.StationId);

            Assert.Equal(new DateTime(2023, 1, 24, 7, 31, 0, DateTimeKind.Utc), tafRecord.IssueTime);

            Assert.Equal(new DateTime(2023, 1, 24, 9, 0, 0, DateTimeKind.Utc), tafRecord.Start);
            Assert.Equal(new DateTime(2023, 1, 25, 15, 0, 0, DateTimeKind.Utc), tafRecord.End);

            WindRecord wind = new()
            {
                WindSpeed = 3d,
                WindGust = 10d,
                WindDirection = "север"
            };
            //Assert.Equal(wind, tafRecord.Wind);
            Assert.Equal(wind.WindDirection, tafRecord.Wind.WindDirection);
            Assert.Equal(wind.WindSpeed, tafRecord.Wind.WindSpeed);
            Assert.Equal(wind.WindGust, tafRecord.Wind.WindGust);

            Assert.Equal("более 10000м", tafRecord.Visibility);
            Assert.Equal("разбросанная, нижн. гран. 910м", string.Join(", ", tafRecord.MainSkies));

            var becmg = tafRecord.Trends[0];
            Assert.Equal("BECMG 2420/2422 06008MPS BKN012CB", becmg.TafText);
            Assert.Equal(new DateTime(2023, 1, 24, 20, 0, 0, DateTimeKind.Utc), becmg.TimeFrom);
            Assert.Equal(new DateTime(2023, 1, 24, 22, 0, 0, DateTimeKind.Utc), becmg.TimeTo);
            Assert.Equal("ветер северо-восток 8м/с, облачность значительная кучево-дождевые облака нижн. гран. 360м", becmg.Info);

            var fm = tafRecord.Trends[1];
            Assert.Equal("FM250300 08013G25MPS 2500 -SHSN BLSN BKN007 OVC015CB", fm.TafText);
            Assert.Equal(new DateTime(2023, 1, 25, 3, 0, 0, DateTimeKind.Utc), fm.TimeFrom);
            Assert.Equal("ветер восток 13м/с с порывами 25м/с, видимость 2500м, ливневой снег (слаб. интенсивн.), низовая метель, облачность значительная нижн. гран. 210м сплошная кучево-дождевые облака нижн. гран. 450м", fm.Info);

            var tempo = tafRecord.Trends[2];
            Assert.Equal("TEMPO 2503/2506 0500 +SHSN BLSN VV002", tempo.TafText);
            Assert.Equal(new DateTime(2023, 1, 25, 3, 0, 0, DateTimeKind.Utc), tempo.TimeFrom);
            Assert.Equal(new DateTime(2023, 1, 25, 6, 0, 0, DateTimeKind.Utc), tempo.TimeTo);
            Assert.Equal("временами видимость 500м, ливневой снег (сильн. интенсивн.), низовая метель, вертикальная видимость 60м", tempo.Info);

            var fm2 = tafRecord.Trends[3];
            Assert.Equal("FM250800 05018G33MPS 0700 +SHSN BLSN VV///", fm2.TafText);
            Assert.Equal(new DateTime(2023, 1, 25, 8, 0, 0, DateTimeKind.Utc), fm2.TimeFrom);
            Assert.Equal("ветер северо-восток 18м/с с порывами 33м/с, видимость 700м, ливневой снег (сильн. интенсивн.), низовая метель, небо закрыто", fm2.Info);
        }
        [Fact]
        public void TafAmdTest1()
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load("../../../tafExamples/AMDCOR/amd.xml");
            var xml = xDoc.OuterXml;

            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var tafs = AWC.ParseTafData(xml);

            Assert.Single(tafs);
            var m = tafs.First();

            var tafRecord = new MeteoRecordFactory(Language.RUS).CreateFrom(m);
            Assert.Equal("TAF AMD USSS 211120Z 2112/2212 11006G16MPS 1600 SHSN BLSN BKN011CB", tafRecord.MainRaw);
            Assert.Equal("USSS", tafRecord.StationId);
        }
        [Fact]
        public void TafUpload()
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load("../../../tafExamples/9.xml");
            var xml = xDoc.OuterXml;
            //var xml = AddHeaders($"<raw_text>UHPP 080430Z VRB01MPS 0099 BKN046CB BKN130TCU 05/M04 Q1011 R26R/010060 NOSIG RMK MT OBSC QFE755/1007</raw_text>");
            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var awc = new AWC(new AwcSourceInternet(), loggerFactory.CreateLogger<AWC>());

            var tafs = AWC.ParseTafData(xml);
            var tafRecord = new MeteoRecordFactory().CreateFrom(tafs.First());
            string iata = "SVX";
            UploadTaf(iata, tafRecord);
            var record = DownloadTaf(iata);
            Assert.Equal(record.RawText, tafRecord.RawText);
        }
        private TafMeteoRecord? DownloadTaf(string iata)
        {
            using var c = CreateConnection("Data Source = (DESCRIPTION = (ADDRESS = (PROTOCOL = tcp)(HOST = 192.168.5.135)(PORT = 1521))(CONNECT_DATA = (SERVICE_NAME = mega10test))); User Id = spp; Password = plan");
            var sql = "select JSON from TAF t where IATA = :0";
            object[] args = new object[] { iata };
            string json = QM.QueryString(c, sql, args);
            return JsonSerializer.Deserialize<TafMeteoRecord>(json);
        }
        private void UploadTaf(string iata, TafMeteoRecord tafRecord)
        {
            using var c = CreateConnection("Data Source = (DESCRIPTION = (ADDRESS = (PROTOCOL = tcp)(HOST = 192.168.5.135)(PORT = 1521))(CONNECT_DATA = (SERVICE_NAME = mega10test))); User Id = spp; Password = plan");
            string json = JsonSerializer.Serialize(tafRecord);
            string sql = "update TAF set JSON = :0 where IATA = :1";
            object[] args = new object[] { json, iata };
            QM.DML(c, sql, args);
        }
        private static OracleConnection CreateConnection(string connectionString)
        {
            OracleConnection oracleConnection = new(connectionString);
            oracleConnection.Open();
            return oracleConnection;
        }
    }
}