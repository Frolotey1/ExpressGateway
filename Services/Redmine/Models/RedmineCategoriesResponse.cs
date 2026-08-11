using System.Text.Json.Serialization;
public class RedmineCategoriesResponse
{
    [JsonPropertyName("issue_categories")]
    public List<RedmineIssueCategory> IssueCategories { get; set; } = new();
}