using Xunit;
using Xunit.Abstractions;

namespace xUnitTests
{
    public class MessageTests
    {

        private readonly ITestOutputHelper output;

        public MessageTests(ITestOutputHelper output)
        {
            this.output = output;
        }


        [Fact]
        public void FlttMessage_0()
        {
            //using var c = new MegatestConnectionFactory().CreateConnection();
            //var message = "<FONT color=\"#FF0000\">Внимание!</FONT> <br> Meteo test <br> <A href=\"http://meteo.ar.int\"><FONT color=\"#777777\">meteo.ar.int</FONT></A>";

            //QM.DML(c, @"insert into event (event_type, custom_text, date_time)
            //             values(5, :0, sys_extract_utc(systimestamp))", message);
        }

    }
}
