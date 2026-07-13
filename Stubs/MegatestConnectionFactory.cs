using Daocore.OracleEngine;
using MeteoLib.Interfaces;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Stubs
{
    internal class MegatestConnectionFactory : IConnectionFactory
    {
        readonly string _cs = "Data Source = (DESCRIPTION = (ADDRESS = (PROTOCOL = tcp)(HOST = 192.168.231.139)(PORT = 1521))(CONNECT_DATA = (SERVICE_NAME = MEGA10TEST))); User Id = test; Password = test";

        public OracleConnection CreateMegaConnection()
        {
            var c = new OracleConnection(_cs);

            c.Open();

            return c;
        }

        public OracleConnection CreateAssetConnection(string asset)
        {
            var c = new OracleConnection(_cs);

            c.Open();

            return c;


        }

    }
}
