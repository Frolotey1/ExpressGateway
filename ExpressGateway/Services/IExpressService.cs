using ExpressGateway.Models;

namespace ExpressGateway.Services;

public interface IExpressService
{
    Task<SendMessageResponse> SendMessageAsync(string chatId, string message, string? asset = null);
    Task<SendMessageResponse> SendToDefaultGroupAsync(string message);
    Task<ChatListResponse> GetChatsAsync(int limit = 50, int offset = 0);
    Task<WebhookResponse> SetWebhookAsync(WebhookRequest request);
    Task<WebhookResponse> GetWebhookAsync();
    Task<bool> DeleteWebhookAsync();
    Task<bool> ProcessWebhookEventAsync(WebhookEvent eventData);
    Task<PingResponse> PingAsync();
}
public class ChatInfo
{
    public string Asset { get; set; } = string.Empty;
    public string ChatId { get; set; } = string.Empty;
    public string? Name { get; set; }
    public bool IsDefault { get; set; }
    public string Id {get; set;} = string.Empty;
    public int MembersCount {get; set;} 
}
