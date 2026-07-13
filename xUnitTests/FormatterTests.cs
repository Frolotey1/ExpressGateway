using MeteoLib;
using MeteoLib.Impl.Delivery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace xUnitTests
{
    public class FormatterTests
    {
        [Fact]
        public void Escapes_0()
        {
            var f = new TelegramFormatter();

            var hello = f.Bold("hello!");

            Assert.Equal("*hello\\!*", hello);
        }


        [Fact]
        public void Escapes_1()
        {
            var f = new TelegramFormatter();

            var hello = f.Bold(".hello\\.");

            Assert.Equal("*\\.hello\\.*", hello);
        }
    }
}
