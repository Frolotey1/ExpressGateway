public interface IRedmineBotService
{
    Task<string> ProcessMessageAsync(string message, string senderName, string? chatId = null);
    Task<string> GetIssueStatusAsync(int issueId);
    Task<string> CreateIssueAsync(string subject, string description, string? categoryId = null);
    Task<string> GetUserIssuesAsync(string? city = null, string? department = null);
    Task<string> GetHelpAsync();
    Task<string> GetGreetingAsync(string? userName = null);
}