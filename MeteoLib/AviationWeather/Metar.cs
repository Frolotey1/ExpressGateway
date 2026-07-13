namespace MeteoLib.AviationWeather
{

#pragma warning disable IDE1006 // Naming Styles

    public class Metar
    {

        public string raw_text { get; set; } = string.Empty;
        public string raw_main { get; set; } = string.Empty;
        public List<MetarTrend> trends { get; set; } = new();
        public string raw_rmk { get; set; } = string.Empty;
        public string station_id { get; set; } = string.Empty;
        public DateTime observation_time { get; set; }
        public double? temp_c { get; set; }
        public bool? temp_c_sing { get; set; }
        public double? dewpoint_c { get; set; }
        public bool? dewpoint_c_sing { get; set; }
        public int? wind_dir_degrees { get; set; }
        public double? wind_speed_kt { get; set; }
        public double? wind_gust_kt { get; set; }
        public bool? wind_changingdir { get; set; }
        public int? visibility_statute_m { get; set; }
        public int? atm_preasure { get; set; }
        public string mountain_visiblity { get; set; } = string.Empty;

        public string wx_string { get; set; } = string.Empty;
        public List<MetarSky> metar_skies { get; set; } = new List<MetarSky>();
        public List<MetarAirStrip> metar_air_strips = new List<MetarAirStrip>();
    }
#pragma warning restore IDE1006 // Naming Styles
}
