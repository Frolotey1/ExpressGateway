namespace ExpressGateway.Models;

public class WebhookRequest
{
    public string SenderId { get; set; } = string.Empty;
    public string ChatId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public DateTime? Timestamp { get; set; }
    public object? Metadata { get; set; }
    public string Url {get; set;} = string.Empty;
    public string Secret {get;set;} = string.Empty;
}
