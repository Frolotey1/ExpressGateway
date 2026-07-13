namespace MeteoLib.Impl.Delivery
{
    public interface IFormatter
    {
        string BaseMeteoUrl { get; }
        object BaseProtocol { get; }

        string ColorText(string text, Color color);
        string Bold(string text);
        string BreakLine();
        string Link(string url, string label);
        string RegularText(string text);
    }


    public enum Color { RED, GRAY };
}