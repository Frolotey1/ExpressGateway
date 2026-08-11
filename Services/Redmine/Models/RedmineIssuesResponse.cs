using System.Text.Json.Serialization;
using ExpressGateway.Services.Redmine.Models;
public class RedmineIssuesResponse
{
    [JsonPropertyName("issues")]
    public List<RedmineIssue> Issues { get; set; } = new();
    
    [JsonPropertyName("total_count")]
    public int TotalCount { get; set; }
    
    public int Limit { get; set; }
    public int Offset { get; set; }
}