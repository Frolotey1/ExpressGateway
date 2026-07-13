using MeteoLib.AviationWeather;

namespace MeteoLib
{
    public class TafMeteoRecord
    {
        public string RawText { get; set; } = string.Empty;
        public string StationId { get; set; } = string.Empty;
        public DateTime IssueTime { get; set; }
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
        public string MainConditions { get; set; } = string.Empty;
        public Temperature? MinTemperature { get; set; }
        public Temperature? MaxTemperature { get; set; }
        public string Visibility { get; set; }
        public string MainRaw { get; set; }
        public WindRecord Wind { get; set; }
        public List<TafTrend> Trends { get; set; } = new();
        public IEnumerable<string> MainSkies { get; set; }
    }

    public class TafTrend
    {
        public WindRecord Wind { get; set; }
        public string Visibility { get; set; }
        public string TafText { get; set; }
        public int? Probability { get; set; }
        public DateTime? TimeFrom { get; set; }
        public DateTime? TimeTo { get; set; }
        public string Info { get; set; } = string.Empty;
        public string Conditions { get; set; } = string.Empty;
        public IEnumerable<string> Skies { get; set; }
        public double? VerticalVisibility { get; set; }
    }
    public class WindRecord
    {
        public string WindDirection { get; set; }
        public double? WindSpeed { get; set; }
        public double? WindGust { get; set; }
    }
}
