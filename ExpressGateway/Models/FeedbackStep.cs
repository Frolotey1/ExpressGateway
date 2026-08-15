public class FeedbackStep
{
    public string Step { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; 
    public string? Message { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}