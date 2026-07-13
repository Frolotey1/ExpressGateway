namespace MeteoLib.AviationWeather
{

#pragma warning disable IDE1006 // Naming Styles

    public class MetarSky
    {
        public string sky_cover { get; set; } = string.Empty;
        public double cloud_base_ft_agl { get; set; }
        public string clouds { get; set; } = string.Empty;
    }

#pragma warning restore IDE1006 // Naming Styles
}
