using System.Text.Json.Serialization;

namespace ExpressGateway.Models;

public class IncomingMessage
{
    public string? Id { get; set; }    
    public string? ChatId { get; set; }   
    public string? Text { get; set; }     
    public string? Body { get; set; }     
    public MessageSender? Sender { get; set; }
}