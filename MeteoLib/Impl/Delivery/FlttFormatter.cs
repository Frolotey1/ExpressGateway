namespace MeteoLib.Impl.Delivery
{
    internal class FlttFormatter : IFormatter
    {
        public string BaseMeteoUrl => "meteo.ar.int";

        public object BaseProtocol => "http";

        public string Bold(string text)
        {
            return $"<b>{text}</b>";
        }

        public string BreakLine()
        {
            return "<br>";
        }

        public string ColorText(string text, Color color)
        {
            var scolor = color switch
            {
                Color.RED => "#FF0000",
                Color.GRAY => "#777777",
                _ => throw new NotImplementedException(color.ToString())
            };

            return $"<font color=\"{scolor}\">{text}</font>";
        }

        public string Link(string url, string label)
        {
            return $"<a href=\"{url}\">{label}</a>";
        }

        public string RegularText(string text)
        {
            return text;
        }
    }
}