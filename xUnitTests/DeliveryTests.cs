using MeteoLib;
using System;
using System.Linq;
using Xunit;
using xUnitTests.Utils;

namespace xUnitTests
{
    public class DeliveryTests
    {
        [Fact]
        public void Delivery_0()
        {
            //arrange

            var ds = Stub.CreateDeliveryService(out var m, out var repo, out _, out _);


            var trg = new Trigger { UnderZero = Level.HIGH };
            repo.SavePacket("a", null, new(), null, trg);

            //act

            ds.CheckAndDeliveryAsset("a");

            //assert          
            var p = repo.GetPacket("a");

            Assert.True(p.IsTriggerComplete);
            Assert.Equal(2, m.Messages.Count());
        }

        [Fact]
        public void Delivery_Empty_1()
        {
            //arrange            
            var ds = Stub.CreateDeliveryService(out var m, out var repo, out _, out var alert);

            //act

            repo.SavePacket("x", null, new MeteoRecord(), null, EmptyTrigger);
            ds.CheckAndDeliveryAsset("x");

            //assert
            Assert.Empty(m.Messages);
            Assert.True(repo.GetPacket("x").IsTriggerComplete);
            //Assert.NotEmpty(alert.errors);

            //Assert.Empty(m.Messages);
            //Assert.True(repo.GetPacket("x").IsTriggerComplete);
        }
        [Fact]
        public void Delivery_Empty_0()
        {
            //arrange
            var mb = Stub.MessageBuilder("x", EmptyTrigger);

            //act
            var message = mb.BuildMessage();

            //assert
            Assert.DoesNotContain("внимание", message, StringComparison.OrdinalIgnoreCase);
        }
        private Trigger EmptyTrigger => new Trigger();        
    }
}
