public class RedmineUser
{
    public int Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{LastName} {FirstName}";
    public string Email { get; set; } = string.Empty;
    public string? ApiKey { get; set; }
    public string? City { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
    public string? Mobile { get; set; }
    public string? LandlinePhone { get; set; }
    public string? OrgActive { get; set; }
}