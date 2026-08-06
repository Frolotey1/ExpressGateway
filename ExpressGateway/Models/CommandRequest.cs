public class CommandRequest
{
    public string Command { get; set; } = string.Empty;
    public string? Sender { get; set; }
    public string? ChatId { get; set; }
}