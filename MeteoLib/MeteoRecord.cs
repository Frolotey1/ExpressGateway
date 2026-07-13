namespace MeteoLib
{
    public class MeteoRecord
    {
        public DateTime DateTime { get; set; }
        public string RawText { get; set; } = string.Empty;
        public string StationId { get; set; } = string.Empty;
        public double? Temperature { get; set; }

        public string TemperatureStr { get; set; } = string.Empty;
        public double? Dewpoint { get; set; }
        public string DewpointStr { get; set; } = string.Empty;

        public double? WindSpeed { get; set; }
        public double? WindGust { get; set; }
        public int? WindDirDegrees { get; set; }
        public bool? ChangingWindDir { get; set; }
        public int? AtmPreasure { get; set; }
        // public double Adhesion { get; set; }
        // public string AirStripNo { get; set; } = string.Empty;
        // public string AdhesionNote { get; set; } = string.Empty;
        public List<AirStrip> AirStrips { get; set; } = new List<AirStrip>();
        public List<string> Conditions { get; set; } = new();
        public int VisiblityValue { get; set; }
        //public bool TriggerTemperature { get; set; }
        public List<string> Skies { get; set; } = new();
        public string Visiblity { get; set; } = string.Empty;
        public string WindDirection { get; set; } = string.Empty;
        public string MountainVisiblity { get; set; } = string.Empty;
        public Dictionary<string, string> ConditionStings { get; set; } = new();
        public List<TrendRecord> Trends { get; set; } = new();
    }


    public class TrendRecord
    {
        public DateTime? TimeFrom { get; set; }
        public DateTime? TimeTo { get; set; }
        public DateTime? TimeAt { get; set; }
        public string Info { get; set; } = string.Empty;
    }

    public class AirStrip
    {
        public double Adhesion { get; set; }
        public string AirStripNo { get; set; } = string.Empty;
        public string AdhesionNote { get; set; } = string.Empty;
    }
}