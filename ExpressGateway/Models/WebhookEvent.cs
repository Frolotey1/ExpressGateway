namespace ExpressGateway.Services;

public class WebhookEvent
{
    public string? Id { get; set; }
    public string? EventType { get; set; }
    public string? ChatId { get; set; }
    public string? MessageId { get; set; }
    public string? UserId { get; set; }
    public DateTime? Timestamp { get; set; }
    public WebhookMessage? Message { get; set; }
    public ChatInfo? ChatInfo { get; set; }
    public UserInfo? UserInfo { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class WebhookMessage
{
    public string? Id { get; set; }
    public string? Text { get; set; }
    public string? SenderId { get; set; }
    public DateTime? SentAt { get; set; }
    public List<string>? Attachments { get; set; }
}

public class UserInfo
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}