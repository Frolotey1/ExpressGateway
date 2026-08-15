namespace ExpressGateway.Models;

public class CommandResult
{
    public string Status { get; set; } = "accepted"; 
    public string Message { get; set; } = string.Empty;
    public string? Command { get; set; }
    public string? Sender { get; set; }
    public object? Data { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? TrackingId { get; set; }
    public List<FeedbackStep>? Steps { get; set; }
}
