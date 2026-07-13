using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MeteoLib.AviationWeather.AwcSource
{
    internal class AwcSourceLocal
    {
        public static bool MetarTurnedOn
        {
            get
            {
                var policy = new CipherSuitesPolicy(
                    new[]
                    {
                        TlsCipherSuite.TLS_AES_256_GCM_SHA384,
                        TlsCipherSuite.TLS_AES_128_GCM_SHA256,
                        TlsCipherSuite.TLS_AES_128_CCM_SHA256,
                        TlsCipherSuite.TLS_AES_128_CCM_8_SHA256
                    }
                    );
                var handler = new SocketsHttpHandler();
                handler.SslOptions.CipherSuitesPolicy = policy;

                var handler1 = new HttpClientHandler();
                handler1.SslProtocols = System.Security.Authentication.SslProtocols.None;
                handler1.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

                using var client = new HttpClient(handler1);

                var response = client.GetAsync("https://home.ar.int/meteo/getxml").Result;

                return response.StatusCode == HttpStatusCode.OK;
            }
        }
        public static bool TafTurnedOn
        {
            get
            {
                var policy = new CipherSuitesPolicy(
                    new[]
                    {
                        TlsCipherSuite.TLS_AES_256_GCM_SHA384,
                        TlsCipherSuite.TLS_AES_128_GCM_SHA256,
                        TlsCipherSuite.TLS_AES_128_CCM_SHA256,
                        TlsCipherSuite.TLS_AES_128_CCM_8_SHA256
                    }
                    );
                var handler = new SocketsHttpHandler();
                handler.SslOptions.CipherSuitesPolicy = policy;

                var handler1 = new HttpClientHandler();
                handler1.SslProtocols = System.Security.Authentication.SslProtocols.None;
                handler1.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

                using var client = new HttpClient(handler1);

                var response = client.GetAsync("https://home.ar.int/meteo/gettaf").Result;

                return response.StatusCode == HttpStatusCode.OK;
            }
        }

        internal static string MetarUrl = "https://home.ar.int/meteo/getxml";

        internal static string TafUrl = "https://home.ar.int/meteo/gettaf";
    }
}
