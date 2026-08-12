using System.Text.Json.Serialization;
public class ExpressCommand
{
    [JsonPropertyName("body")]
    public string? Body { get; set; }

    [JsonPropertyName("command_type")]
    public string? CommandType { get; set; }

    [JsonPropertyName("data")]
    public Dictionary<string, object>? Data { get; set; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }
}