using System.Text.Json.Serialization;

namespace ExpressGateway.Models;

public class ExpressApiResponse
{
    [JsonPropertyName("messageId")]
    public string? MessageId { get; set; }
    
    [JsonPropertyName("status")]
    public string? Status { get; set; }
    
    [JsonPropertyName("error")]
    public string? Error { get; set; } 
    
    [JsonPropertyName("chatId")]
    public string? ChatId { get; set; }
}