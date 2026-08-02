namespace ExpressGateway.Models;

public class SendMessageResponse
{
    public bool Success { get; set; }
    public string? MessageId { get; set; }
    public string? ChatId { get; set; }
    public DateTime? SentAt { get; set; }
    public string? Status { get; set; }
    public string? Error { get; set; }
    public string? ExpressResponse { get; set; }
}
