namespace MeteoLib.Interfaces
{
    public interface IAlert
    {
        Task SendAsync(string issueText);
    }
}
