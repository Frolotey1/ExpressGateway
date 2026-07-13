using MeteoLib;
using MeteoLib.AviationWeather;
using System;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace xUnitTests
{
    public class TriggerTests
    {

        private readonly ITestOutputHelper output;

        public TriggerTests(ITestOutputHelper output)
        {
            this.output = output;
        }


        [Fact]
        public void Condition_0()
        {
            var oldm = new MeteoRecordFactory().CreateFrom(new Metar { wx_string = "-SHSN" });
            var newm = new MeteoRecordFactory().CreateFrom(new Metar { wx_string = "-SHRA -SHSN" });
            var triggers = TriggerBuilder.Detect(oldm, newm);
            output.WriteLine(JsonSerializer.Serialize(newm));

            Assert.Equal(Level.HIGH, triggers.Conditions["-SHRA"]);
            Assert.Equal(Level.LOW, triggers.Conditions["-SHSN"]);
        }

        [Fact]
        public void Condition_1()
        {
            var oldm = new MeteoRecordFactory().CreateFrom(new Metar { wx_string = "-SHSN" });
            var newm = new MeteoRecordFactory().CreateFrom(new Metar { wx_string = "-SHRA SHSN" });
            var triggers = TriggerBuilder.Detect(oldm, newm);
            output.WriteLine(JsonSerializer.Serialize(newm));

            Assert.Equal(Level.HIGH, triggers.Conditions["-SHRA"]);
            Assert.Equal(Level.HIGH, triggers.Conditions["SHSN"]);
        }

        [Fact]
        public void Condition_2()
        {
            var oldm = new MeteoRecordFactory().CreateFrom(new Metar { wx_string = "SHSN" });
            var newm = new MeteoRecordFactory().CreateFrom(new Metar { wx_string = "-SHRA -SHSN" });
            var triggers = TriggerBuilder.Detect(oldm, newm);
            output.WriteLine(JsonSerializer.Serialize(newm));

            Assert.Equal(Level.HIGH, triggers.Conditions["-SHRA"]);
            Assert.Equal(Level.LOW, triggers.Conditions["-SHSN"]);
        }

        [Theory]
        [InlineData(-31, -31, Level.LOW)]
        [InlineData(-28, -31, Level.HIGH)]
        [InlineData(-28, -29, Level.NONE)]
        [InlineData(28, 29, Level.NONE)]
        [InlineData(null, 29, Level.NONE)]
        [InlineData(null, -31, Level.HIGH)]
        public void Temp_0(double? t1, double t2, Level level)
        {
            MeteoRecord? oldm = null;
            if (t1 != null)
                oldm = new MeteoRecordFactory().CreateFrom(new Metar { temp_c = Math.Abs((double)t1), temp_c_sing = (double)t1 >= 0 });
            var newm = new MeteoRecordFactory().CreateFrom(new Metar { temp_c = Math.Abs(t2), temp_c_sing = t2 >= 0 });
            var triggers = TriggerBuilder.Detect(oldm, newm);
            output.WriteLine(JsonSerializer.Serialize(newm));

            Assert.Equal(level, triggers.ColdLimit);
        }

        [Theory]
        [InlineData(31, 31, Level.LOW)]
        [InlineData(28, 31, Level.HIGH)]
        [InlineData(28, 29, Level.NONE)]
        [InlineData(-28, -29, Level.NONE)]
        [InlineData(null, -29, Level.NONE)]
        [InlineData(null, 31, Level.HIGH)]
        public void Temp_1(double? t1, double t2, Level level)
        {
            MeteoRecord? oldm = null;
            if (t1 != null)
                oldm = new MeteoRecordFactory().CreateFrom(new Metar { temp_c = Math.Abs((double)t1), temp_c_sing = (double)t1 >= 0 });
            var newm = new MeteoRecordFactory().CreateFrom(new Metar { temp_c = Math.Abs(t2), temp_c_sing = t2 >= 0 });
            var triggers = TriggerBuilder.Detect(oldm, newm);
            output.WriteLine(JsonSerializer.Serialize(newm));

            Assert.Equal(level, triggers.WarmLimit);
        }

        [Theory]
        [InlineData(31, false, null, null, Level.NONE)]
        [InlineData(28, true, null, null, Level.NONE)]
        [InlineData(null, null, 29, true, Level.NONE)]
        [InlineData(null, null, 31, false, Level.HIGH)]
        public void Temp_2(double? t1, bool? t1_sign, double? t2, bool? t2_sign, Level level)
        {
            var oldm = new MeteoRecordFactory().CreateFrom(new Metar { temp_c = t1, temp_c_sing = t1_sign });
            var newm = new MeteoRecordFactory().CreateFrom(new Metar { temp_c = t2, temp_c_sing = t2_sign });
            var triggers = TriggerBuilder.Detect(oldm, newm);
            output.WriteLine(JsonSerializer.Serialize(newm));

            Assert.Equal(level, triggers.ColdLimit);
        }

        //граничная скорость 29,1577 узлов или 15 м/с
        [Theory]
        [InlineData(31, 31, Level.LOW)]
        [InlineData(28, 31, Level.HIGH)]
        [InlineData(27, 28, Level.NONE)]
        [InlineData(null, -29, Level.NONE)]
        [InlineData(null, 31, Level.HIGH)]
        public void Wind_0(double? t1, double t2, Level level)
        {
            MeteoRecord? oldm = null;
            if (t1 != null)
                oldm = new MeteoRecordFactory().CreateFrom(new Metar { wind_speed_kt = (double)t1 });
            var newm = new MeteoRecordFactory().CreateFrom(new Metar { wind_speed_kt = (double)t2 });
            var triggers = TriggerBuilder.Detect(oldm, newm);
            output.WriteLine(JsonSerializer.Serialize(oldm));
            output.WriteLine(JsonSerializer.Serialize(newm));

            Assert.Equal(level, triggers.WindSpeedLimit);
        }


        //граничная скорость 0.6215 мили узлов или 1000 м
        [Theory]
        [InlineData(900, 990, Level.LOW)]
        [InlineData(1100, 990, Level.HIGH)]
        [InlineData(1100, 1200, Level.NONE)]
        [InlineData(null, 1100, Level.NONE)]
        [InlineData(null, 990, Level.HIGH)]
        public void Visibl_0(int? t1, int t2, Level level)
        {
            MeteoRecord? oldm = null;
            if (t1 != null)
                oldm = new MeteoRecordFactory().CreateFrom(new Metar { visibility_statute_m = (int)t1 });
            var newm = new MeteoRecordFactory().CreateFrom(new Metar { visibility_statute_m = t2 });
            var triggers = TriggerBuilder.Detect(oldm, newm);
            output.WriteLine(JsonSerializer.Serialize(oldm));
            output.WriteLine(JsonSerializer.Serialize(newm));

            Assert.Equal(level, triggers.VisiblityLimit);
        }

        [Fact]
        public void DeliveryStatus_0()
        {
            var trg = Trigger.CreateEmpty();

            Assert.False(trg.DeliveryStatus());
        }


        [Fact]
        public void DeliveryStatus_1()
        {
            var trg = Trigger.CreateEmpty();

            trg.UnderZero = Level.HIGH;

            Assert.True(trg.DeliveryStatus());
        }


        [Fact]
        public void DeliveryStatus_2()
        {
            var trg = Trigger.CreateEmpty();

            trg.WarmLimit = Level.LOW;

            Assert.False(trg.DeliveryStatus());
        }

        [Fact]
        public void DeliveryStatus_3()
        {
            var trg = Trigger.CreateEmpty();

            trg.Conditions.Add("a", Level.HIGH);

            Assert.True(trg.DeliveryStatus());
        }


        [Fact]
        public void DeliveryStatus_4()
        {
            var trg = Trigger.CreateEmpty();

            trg.Conditions.Add("a", Level.LOW);

            Assert.False(trg.DeliveryStatus());
        }
    }
}
