using System.Text.Json.Serialization;
public class RedmineOrgActivesResponse
{
    [JsonPropertyName("org_actives")]
    public List<RedmineOrgActive> OrgActives { get; set; } = new();
}