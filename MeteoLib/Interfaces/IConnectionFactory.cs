using Oracle.ManagedDataAccess.Client;

namespace MeteoLib.Interfaces
{
    public interface IConnectionFactory
    {
        OracleConnection CreateMegaConnection();

        OracleConnection CreateAssetConnection(string asset);
    }
}
