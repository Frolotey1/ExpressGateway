using MeteoLib.AviationWeather;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace xUnitTests.TimeTests
{
    public class TafTemperatureTimeTests
    {
        [Fact]
        public void TEmperatureTestd()
        {
            var rawtext = @"TX12/2515Z TN14/1624Z";
            DateTime createdTime = new(2023, 5, 16, 14, 3, 0, DateTimeKind.Utc);
            var (_, min) = TafParser.GetTemperature(rawtext, createdTime);

            var DateTime = new DateTime(2023, 5, 17, 0, 0, 0, DateTimeKind.Utc);

            Assert.Equal(DateTime, min.DateTime);
        }
        [Fact]
        public void TEmperatureTestm()
        {
            var rawtext = @"TX12/2515Z TN14/3124Z";
            DateTime createdTime = new(2023, 5, 31, 14, 4, 0, DateTimeKind.Utc);
            var (_, min) = TafParser.GetTemperature(rawtext, createdTime);

            var DateTime = new DateTime(2023, 6, 1, 0, 0, 0, DateTimeKind.Utc);

            Assert.Equal(DateTime, min.DateTime);
        }
        [Fact]
        public void TEmperatureTesty()
        {
            var rawtext = @"TX12/2515Z TN14/3124Z";
            DateTime createdTime = new(2023, 12, 31, 14, 5, 0, DateTimeKind.Utc);
            var (_, min) = TafParser.GetTemperature(rawtext, createdTime);

            var DateTime = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            Assert.Equal(DateTime, min.DateTime);
        }
    }
}
