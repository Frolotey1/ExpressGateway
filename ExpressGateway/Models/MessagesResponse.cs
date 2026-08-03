using ExpressGateway.Models;
public class MessagesResponse
{
    public List<IncomingMessage> Messages { get; set; } = new();
    public int Total { get; set; }
    public bool HasMore { get; set; }
}