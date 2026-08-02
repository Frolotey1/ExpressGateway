namespace ExpressGateway.Models;

public class SendMessageRequest
{
    public string ChatId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Asset { get; set; }
    public MessagePriority? Priority { get; set; }
}

public enum MessagePriority
{
    Normal,
    High,
    Urgent
}
