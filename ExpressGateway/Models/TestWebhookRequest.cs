public class TestWebhookRequest
{
    public string Command { get; set; } = "/help";
    public string? Sender { get; set; } = "test-user";
    public string? ChatId { get; set; } = "test-chat";
}