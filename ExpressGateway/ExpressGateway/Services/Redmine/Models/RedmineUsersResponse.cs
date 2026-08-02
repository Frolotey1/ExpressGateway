using System.Text.Json.Serialization;
public class RedmineUsersResponse
{
    [JsonPropertyName("users")]
    public List<RedmineUser> Users { get; set; } = new();
    
    [JsonPropertyName("total_count")]
    public int TotalCount { get; set; }
    
    public int Limit { get; set; }
    public int Offset { get; set; }
}