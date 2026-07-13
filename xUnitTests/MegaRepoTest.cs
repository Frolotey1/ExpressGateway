using Meteohost.Factories;
using MeteoLib;
using Microsoft.Extensions.Logging.Abstractions;
using Stubs;
using System;
using System.Linq;
using Xunit;

namespace xUnitTests
{
    public class MegaRepoTest
    {
        readonly DateTime D0 = new(2022, 2, 2);
        readonly DateTime D1 = new(2022, 2, 3);
        readonly Mega10Repository R = new(NullLogger<Mega10Repository>.Instance, TF.MegaConnection(), new AirportsConst());



        [Fact]
        public void Save_0()
        {
            var code = RandomCode();




            var c = new MeteoRecord
            {
                DateTime = D0,
                Temperature = 15
            };

            R.SavePacket(code, null, c, null, new Trigger());


            var p = R.GetPacket(code);

            Assert.NotNull(p);
            Assert.Equal(D0, p.CurrentMeteo.DateTime);
            Assert.Equal(15, p.CurrentMeteo.Temperature);
            //Assert.Equal(code, p.IataCode);

        }



        [Fact]
        public void Save_1()
        {
            var code = RandomCode();


            var d0 = new DateTime(2022, 2, 2);
            var d1 = new DateTime(2022, 2, 3);


            var pr = new MeteoRecord { DateTime = d0 };
            var cu = new MeteoRecord { DateTime = d1 };
            var tr = new Trigger { UnderZero = Level.HIGH };

            R.SavePacket(code, pr, cu, cu, tr);


            var p = R.GetPacket(code);

            Assert.NotNull(p);
            Assert.Equal(d0, p.PreviousMeteo.DateTime);
            Assert.Equal(d1, p.CurrentMeteo.DateTime);
            Assert.Equal(Level.HIGH, p.Trigger.UnderZero);
        }


        [Fact]
        public void Save_2()
        {
            var code = RandomCode();


            var d0 = new DateTime(2022, 2, 2);
            var d1 = new DateTime(2022, 2, 3);


            var pr = new MeteoRecord { DateTime = d0 };
            var cu = new MeteoRecord { DateTime = d1 };
            var tr = new Trigger { UnderZero = Level.HIGH };

            var sn0 = R.SavePacket(code, pr, cu, cu, tr);

            R.TriggerComplete(sn0);

            var sn1 = R.SavePacket(code, pr, cu, cu, tr);

            var p = R.GetPacket(code);

            Assert.NotEqual(sn0, sn1);
            Assert.Equal(sn1, p.SN);
            Assert.False(p.IsTriggerComplete);

        }


        [Fact]
        public void Sent_0()
        {
            var code = RandomCode();



            var pr = new MeteoRecord { DateTime = D0 };
            var cu = new MeteoRecord { DateTime = D1 };
            var tr = new Trigger { UnderZero = Level.HIGH };

            var sn = R.SavePacket(code, pr, cu, cu, tr);

            R.TriggerComplete(sn);

            var p = R.GetPacket(code);


            Assert.True(p.IsTriggerComplete);

        }

        [Fact]
        public void Unprocessed()
        {
            //arrange
            var code0 = RandomCode();
            var code1 = new AirportsConst().CodeIataList.First();


            var d0 = new DateTime(2022, 2, 2);


            var cu = new MeteoRecord { DateTime = d0 };
            var tr = new Trigger { UnderZero = Level.HIGH };


            //act

            var sn0 = R.SavePacket(code0, null, cu, null, tr);
            var sn1 = R.SavePacket(code1, null, cu, null, tr);

            var list = R.Unprocessed();

            //asssert


            Assert.Contains(list, a => a == code1);
            Assert.DoesNotContain(list, a => a == code0);
        }


        private static readonly Random random = new();
        private static string RandomCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var s = new string(Enumerable.Repeat(chars, 5)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            return $"_{s}";
        }
    }
}
