namespace MeteoLib
{
    public class MeteoException : Exception
    {
        public MeteoException(string? message) : base(message)
        {
        }
    }

    public class MeteoEmptyException : Exception
    {
        public MeteoEmptyException(string? message) : base(message)
        {
        }
    }
}
