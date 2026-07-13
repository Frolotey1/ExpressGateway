using MeteoLib.AviationWeather;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace xUnitTests.TimeTests
{
    public class TafTimeTests
    {
        //TX12/2515Z TN09/2524Z TEMPO 2515/2519
        [Fact]
        public void TimeTest()
        {
            var rawtext = @"TAF UWSG 251400Z 2515/2524 15003G09MPS 6000 BKN016 TX12/2515Z TN09/2524Z TEMPO 2515/2519 17008MPS 0600 +SHRA FEW004 BKN007 BKN016CB";
            //2023-04-25T14:00:00Z
            DateTime createdTime = new(2023, 4, 25, 14, 0, 0, DateTimeKind.Utc);
            var temperatures = TafParser.GetTemperature(rawtext, createdTime);
            Temperature max = new()
            {
                DateTime = new DateTime(2023, 4, 25, 15, 0, 0, DateTimeKind.Utc),
                TemperatureValue = 12,
                UnderZero = false
            };
            Assert.Equal(max.DateTime, temperatures.max.DateTime);
            Assert.Equal(max.TemperatureValue, temperatures.max.TemperatureValue);
            Assert.Equal(max.UnderZero, temperatures.max.UnderZero);

            Temperature min = new()
            {
                DateTime = new DateTime(2023, 4, 26, 0, 0, 0, DateTimeKind.Utc),
                TemperatureValue = 9,
                UnderZero = false
            };
            Assert.Equal(min.DateTime, temperatures.min.DateTime);
            Assert.Equal(min.TemperatureValue, temperatures.min.TemperatureValue);
            Assert.Equal(min.UnderZero, temperatures.min.UnderZero);
        }
        [Fact]
        public void TimeTest2()
        {
            var rawtext = @"TAF UWSG 251400Z 2515/2524 15003G09MPS 6000 BKN016 TX12/3115Z TN09/3124Z TEMPO 2515/2519 17008MPS 0600 +SHRA FEW004 BKN007 BKN016CB";
            //2023-04-25T14:00:00Z
            DateTime createdTime = new(2022, 12, 31, 14, 0, 0, DateTimeKind.Utc);
            var temperatures = TafParser.GetTemperature(rawtext, createdTime);
            Temperature max = new()
            {
                DateTime = new DateTime(2022, 12, 31, 15, 0, 0, DateTimeKind.Utc),
                TemperatureValue = 12,
                UnderZero = false
            };
            Assert.Equal(max.DateTime, temperatures.max.DateTime);
            Assert.Equal(max.TemperatureValue, temperatures.max.TemperatureValue);
            Assert.Equal(max.UnderZero, temperatures.max.UnderZero);

            Temperature min = new()
            {
                DateTime = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                TemperatureValue = 9,
                UnderZero = false
            };
            Assert.Equal(min.DateTime, temperatures.min.DateTime);
            Assert.Equal(min.TemperatureValue, temperatures.min.TemperatureValue);
            Assert.Equal(min.UnderZero, temperatures.min.UnderZero);
        }
        [Fact]
        public void TimeTempoTest()
        {
            var rawtext = @"TEMPO 2515/2524";
            //2023-04-25T14:00:00Z
            DateTime now = new(2022, 12, 25, 0, 0, 0, DateTimeKind.Utc);
            var timeLimit = TafParser.GetTafTimeLimits(rawtext, now).ToArray();

            var start = new DateTime(2022, 12, 25, 15, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2022, 12, 26, 0, 0, 0, DateTimeKind.Utc);

            Assert.Equal(start, timeLimit[0]);
            Assert.Equal(end, timeLimit[1]);
        }
        [Fact]
        public void TimeTempoTest2()
        {
            var rawtext = @"TEMPO 3115/3124";
            //2023-04-25T14:00:00Z
            DateTime now = new(2022, 12, 31, 0, 0, 0, DateTimeKind.Utc);
            var timeLimit = TafParser.GetTafTimeLimits(rawtext, now).ToArray();

            var start = new DateTime(2022, 12, 31, 15, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            Assert.Equal(start, timeLimit[0]);
            Assert.Equal(end, timeLimit[1]);
        }
        [Fact]
        public void TimeTempoTest3()
        {
            var rawtext = @"TEMPO 3115/3124";
            //2023-04-25T14:00:00Z
            DateTime now = new(2022, 12, 26, 0, 0, 0, DateTimeKind.Utc);
            var timeLimit = TafParser.GetTafTimeLimits(rawtext, now).ToArray();

            var start = new DateTime(2022, 12, 31, 15, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            Assert.Equal(start, timeLimit[0]);
            Assert.Equal(end, timeLimit[1]);
        }
        [Fact]
        public void TimeFMTest()
        {
            var rawtext = @"FM252400";
            //2023-04-25T14:00:00Z
            DateTime now = new(2022, 12, 25, 0, 0, 0, DateTimeKind.Utc);
            var timeLimit = TafParser.GetTafTimeLimits(rawtext, now).ToArray();

            var start = new DateTime(2022, 12, 26, 0, 0, 0, DateTimeKind.Utc);

            Assert.Equal(start, timeLimit[0]);
        }
        [Fact]
        public void TimeFMTest2()
        {
            var rawtext = @"FM252360";
            //2023-04-25T14:00:00Z
            DateTime now = new(2022, 12, 25, 0, 0, 0, DateTimeKind.Utc);
            var timeLimit = TafParser.GetTafTimeLimits(rawtext, now).ToArray();

            var start = new DateTime(2022, 12, 26, 0, 0, 0, DateTimeKind.Utc);

            Assert.Equal(start, timeLimit[0]);
        }
        [Fact]
        public void TimeFMTest3()
        {
            var rawtext = @"FM312360";
            //2023-04-25T14:00:00Z
            DateTime now = new(2022, 12, 25, 0, 0, 0, DateTimeKind.Utc);
            var timeLimit = TafParser.GetTafTimeLimits(rawtext, now).ToArray();

            var start = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            Assert.Equal(start, timeLimit[0]);
        }
        [Fact]
        public void TimeFMTest4()
        {
            var rawtext = @"FM312400";
            //2023-04-25T14:00:00Z
            DateTime now = new(2022, 12, 25, 0, 0, 0, DateTimeKind.Utc);
            var timeLimit = TafParser.GetTafTimeLimits(rawtext, now).ToArray();

            var start = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            Assert.Equal(start, timeLimit[0]);
        }
        [Fact]
        public void TimeFMTest5()
        {
            var rawtext = @"FM302360";
            //2023-04-25T14:00:00Z
            DateTime now = new(2022, 11, 30, 0, 0, 0, DateTimeKind.Utc);
            var timeLimit = TafParser.GetTafTimeLimits(rawtext, now).ToArray();

            var start = new DateTime(2022, 12, 1, 0, 0, 0, DateTimeKind.Utc);

            Assert.Equal(start, timeLimit[0]);
        }
        [Fact]
        public void TimeFMTest6()
        {
            var rawtext = @"FM142400";
            //2023-04-25T14:00:00Z
            DateTime now = new(2023, 5, 14, 20, 4, 0, DateTimeKind.Utc);
            var timeLimit = TafParser.GetTafTimeLimits(rawtext, now).ToArray();

            var start = new DateTime(2023, 5, 15, 0, 0, 0, DateTimeKind.Utc);

            Assert.Equal(start, timeLimit[0]);
        }

    }
}
