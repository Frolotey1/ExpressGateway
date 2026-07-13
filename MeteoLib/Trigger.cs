namespace MeteoLib
{
    public enum Level { NONE, LOW, HIGH };

    public class Trigger
    {
        public Level UnderZero { get; set; } = Level.NONE;

        public Level WarmLimit { get; set; } = Level.NONE;

        public Level ColdLimit { get; set; } = Level.NONE;

        public Level WindSpeedLimit { get; set; } = Level.NONE;
        public Level VisiblityLimit { get; set; } = Level.NONE;

        public Dictionary<string, Level> Conditions { get; set; } = new();
        public bool DeliveryStatus()
        {
            if (UnderZero == Level.HIGH) return true;
            if (WarmLimit == Level.HIGH) return true;
            if (ColdLimit == Level.HIGH) return true;
            if (WindSpeedLimit == Level.HIGH) return true;
            if (VisiblityLimit == Level.HIGH) return true;
            if (Conditions.Values.Any(c => c == Level.HIGH)) return true;


            return false;
        }

        public static Trigger CreateEmpty() => new();
    }


}
