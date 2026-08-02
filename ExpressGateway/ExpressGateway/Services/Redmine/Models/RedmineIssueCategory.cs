public class RedmineIssueCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? ProjectId { get; set; }
    public int? AssignedToId { get; set; }
    public string? Description { get; set; }
}