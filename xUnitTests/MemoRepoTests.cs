using MeteoLib;
using MeteoLib.Impl.Repo;
using Xunit;

namespace xUnitTests;

public class MemoRepoTests
{

    [Fact]
    public void Complete_0()
    {
        var r = new MemoRepo();

        var c = new MeteoRecord();


        var sn = r.SavePacket("A", c, c, null, null);

        r.TriggerComplete(sn);

        Assert.True(r.GetPacket("A").IsTriggerComplete);


    }
}
