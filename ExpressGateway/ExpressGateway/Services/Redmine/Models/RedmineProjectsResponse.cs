using System.Text.Json.Serialization;
public class RedmineProjectsResponse
{
    [JsonPropertyName("projects")]
    public List<RedmineProject> Projects { get; set; } = new();
    
    [JsonPropertyName("total_count")]
    public int TotalCount { get; set; }
}