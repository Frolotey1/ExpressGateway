namespace ExpressGateway.Models;

public class WebhookResponse
{
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string WebhookUrl {get; set;} = string.Empty;
}