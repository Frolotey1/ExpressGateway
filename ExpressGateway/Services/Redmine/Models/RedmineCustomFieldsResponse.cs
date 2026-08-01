using System.Text.Json.Serialization;
public class RedmineCustomFieldsResponse
{
    [JsonPropertyName("custom_fields")]
    public List<RedmineCustomField> CustomFields { get; set; } = new();
}