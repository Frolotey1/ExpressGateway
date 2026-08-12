using System.Text.Json.Serialization;
public class ExpressCommandRequest
{
    [JsonPropertyName("sync_id")]
    public string? SyncId { get; set; }

    [JsonPropertyName("source_sync_id")]
    public string? SourceSyncId { get; set; }

    [JsonPropertyName("command")]
    public ExpressCommand? Command { get; set; }
}