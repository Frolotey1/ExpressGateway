namespace MeteoLib.Interfaces
{
    public interface IMessenger
    {
        void Send(string asset, string message);
    }
}
