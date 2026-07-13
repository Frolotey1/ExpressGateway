using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteoLib.AviationWeather.AwcSource
{
    public class AwcSourceConfig
    {
        private readonly IConfiguration _config;

        public AwcSourceConfig(IConfiguration config)
        {
            _config = config;
        }

        public IAwcSource GetSource()
        {
            var src = _config.GetValue<string>("AwcSource");

            if (src == "Internet") return new AwcSourceInternet();
            if (src == "LocalAndInternet") return new AwcSourceLocalAndInternet();

            throw new NotImplementedException();
        }
    }
}
