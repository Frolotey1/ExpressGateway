namespace ExpressGateway.Services.Redmine.Models;

public class RedmineIssue
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RedmineProject? Project { get; set; }
    public RedmineTracker? Tracker { get; set; }
    public RedmineStatus? Status { get; set; }
    public RedminePriority? Priority { get; set; }
    public RedmineUser? Author { get; set; }
    public RedmineUser? AssignedTo { get; set; }
    public int? CategoryId { get; set; } 
    public RedmineIssueCategory? Category { get; set; }
    public List<RedmineCustomField> CustomFields { get; set; } = new();
    public List<RedmineAttachment> Attachments { get; set; } = new();
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
}