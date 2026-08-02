using System.Text.Json.Serialization;

namespace ExpressGateway.Models;

public class MessageSender
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }
}