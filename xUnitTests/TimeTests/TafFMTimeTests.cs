using MeteoLib.AviationWeather;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace xUnitTests.TimeTests
{
    public class TafFMTimeTests
    {
        [Fact]
        public void TimeFMTestd()
        {
            var rawtext = @"FM162400";

            DateTime now = new(2023, 5, 16, 14, 3, 0, DateTimeKind.Utc);
            var timeLimit = TafParser.GetTafTimeLimits(rawtext, now).ToArray();

            var start = new DateTime(2023, 5, 17, 0, 0, 0, DateTimeKind.Utc);

            Assert.Equal(start, timeLimit[0]);
        }
        [Fact]
        public void TimeFMTestm()
        {
            var rawtext = @"FM312400";

            DateTime now = new(2023, 5, 31, 14, 5, 0, DateTimeKind.Utc);
            var timeLimit = TafParser.GetTafTimeLimits(rawtext, now).ToArray();

            var start = new DateTime(2023, 6, 1, 0, 0, 0, DateTimeKind.Utc);

            Assert.Equal(start, timeLimit[0]);
        }
        [Fact]
        public void TimeFMTesty()
        {
            var rawtext = @"FM312400";

            DateTime now = new(2023, 12, 31, 14, 4, 0, DateTimeKind.Utc);
            var timeLimit = TafParser.GetTafTimeLimits(rawtext, now).ToArray();

            var start = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            Assert.Equal(start, timeLimit[0]);
        }
        [Fact]
        public void TimeFMTestdh()
        {
            var rawtext = @"FM162360";

            DateTime now = new(2023, 5, 16, 14, 3, 0, DateTimeKind.Utc);
            var timeLimit = TafParser.GetTafTimeLimits(rawtext, now).ToArray();

            var start = new DateTime(2023, 5, 17, 0, 0, 0, DateTimeKind.Utc);

            Assert.Equal(start, timeLimit[0]);
        }
        [Fact]
        public void TimeFMTestmh()
        {
            var rawtext = @"FM312360";

            DateTime now = new(2023, 5, 31, 14, 4, 0, DateTimeKind.Utc);
            var timeLimit = TafParser.GetTafTimeLimits(rawtext, now).ToArray();

            var start = new DateTime(2023, 6, 1, 0, 0, 0, DateTimeKind.Utc);

            Assert.Equal(start, timeLimit[0]);
        }
        [Fact]
        public void TimeFMTestyh()
        {
            var rawtext = @"FM312360";

            DateTime now = new(2023, 12, 31, 14, 5, 0, DateTimeKind.Utc);
            var timeLimit = TafParser.GetTafTimeLimits(rawtext, now).ToArray();

            var start = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            Assert.Equal(start, timeLimit[0]);
        }
    }
}
