using MeteoLib.Impl.Repo;
using MeteoLib.Interfaces;

namespace Stubs;

internal class TF
{
    public IMeteoAPI Api { get; } = new TestApi();
    public IRepo Repo { get; } = new MemoRepo();

    internal static IConnectionFactory MegaConnection() => new MegatestConnectionFactory();
}
