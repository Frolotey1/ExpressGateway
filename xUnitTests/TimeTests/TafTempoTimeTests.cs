using MeteoLib.AviationWeather;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace xUnitTests.TimeTests
{
    public class TafTempoTimeTests
    {
        [Fact]
        public void TimeTempoTestd()
        {
            var rawtext = @"TEMPO 1615/1624";
            //2023-04-25T14:00:00Z
            DateTime now = new(2023, 5, 16, 14, 3, 0, DateTimeKind.Utc);
            var timeLimit = TafParser.GetTafTimeLimits(rawtext, now).ToArray();

            var start = new DateTime(2023, 5, 16, 15, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2023, 5, 17, 0, 0, 0, DateTimeKind.Utc);

            Assert.Equal(start, timeLimit[0]);
            Assert.Equal(end, timeLimit[1]);
        }
        [Fact]
        public void TimeTempoTestm()
        {
            var rawtext = @"TEMPO 3115/3124";
            //2023-04-25T14:00:00Z
            DateTime now = new(2023, 5, 31, 14, 4, 0, DateTimeKind.Utc);
            var timeLimit = TafParser.GetTafTimeLimits(rawtext, now).ToArray();

            var start = new DateTime(2023, 5, 31, 15, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2023, 6, 1, 0, 0, 0, DateTimeKind.Utc);

            Assert.Equal(start, timeLimit[0]);
            Assert.Equal(end, timeLimit[1]);
        }
        [Fact]
        public void TimeTempoTesty()
        {
            var rawtext = @"TEMPO 3115/3124";
            //2023-04-25T14:00:00Z
            DateTime now = new(2023, 12, 31, 14, 4, 0, DateTimeKind.Utc);
            var timeLimit = TafParser.GetTafTimeLimits(rawtext, now).ToArray();

            var start = new DateTime(2023, 12, 31, 15, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            Assert.Equal(start, timeLimit[0]);
            Assert.Equal(end, timeLimit[1]);
        }
    }
}
