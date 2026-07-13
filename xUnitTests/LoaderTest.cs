using MeteoLib.LoadService;
using Microsoft.Extensions.Logging.Abstractions;
using Stubs;
using System.Threading.Tasks;
using Xunit;

namespace xUnitTests
{
    public class LoaderTest
    {
        [Fact]
        public async Task Test_0()
        {
            var A = new TF();
            

            var l = new LoadService(A.Api, A.Repo, new NullLogger<LoadService>());

            await l.LoadMetarAsync("AAA");

            var p = A.Repo.GetPacket("AAA");

            Assert.NotNull(p);

        }


        [Fact]
        public async Task Many_0()
        {
            var A = new TF();


            var l = new LoadService(A.Api, A.Repo, new NullLogger<LoadService>());

            await l.LoadMetarAsync("AAA");
            await l.LoadMetarAsync("BBB");

            var pa = A.Repo.GetPacket("AAA");
            var pb = A.Repo.GetPacket("BBB");

            Assert.NotNull(pa);
            Assert.NotNull(pb);

            //Assert.Equal("AAA", pa.IataCode);
            //Assert.Equal("BBB", pb.IataCode);

        }
    }
}