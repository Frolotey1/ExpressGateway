namespace MeteoLib.AviationWeather
{

#pragma warning disable IDE1006 // Naming Styles
    public enum TrendType
    {
        TEMPO, BECMG
    }

    public class MetarTrend
    {
        public TrendType trend_type { get; set; }
        public string trand_raw { get; set; } = string.Empty;
        public DateTime? time_from { get; set; }
        public DateTime? time_to { get; set; }
        public DateTime? time_at { get; set; }
        public int? wind_dir_degrees { get; set; }
        public double? wind_speed { get; set; }
        public double? wind_gust { get; set; }
        public bool? wind_changingdir { get; set; }
        public int? visibility_statute_m { get; set; }
        public int? vertical_visiblity_ft { get; set; }
        public List<MetarSky> metar_skies { get; set; } = new List<MetarSky>();
        public string wx_string { get; set; } = string.Empty;

    }

#pragma warning restore IDE1006 // Naming Styles
}
