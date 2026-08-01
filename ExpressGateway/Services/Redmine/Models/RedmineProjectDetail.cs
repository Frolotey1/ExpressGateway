using System.Text.Json.Serialization;
public class RedmineProjectDetail : RedmineProject
{
    [JsonPropertyName("issue_custom_fields")]
    public List<RedmineCustomField> IssueCustomFields { get; set; } = new();
}