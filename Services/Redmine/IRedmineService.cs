namespace ExpressGateway.Services.Redmine.Models;
public interface IRedmineService
{
    Task<RedmineUser?> GetUserByApiKeyAsync(string apiKey);
    Task<RedmineUser?> GetUserByFullNameAsync(string firstName, string lastName, string? city = null);
    Task<RedmineUser> CreateUserAsync(RedmineUser user);
    Task<RedmineIssue?> GetIssueAsync(int id);
    Task<RedmineIssue> CreateIssueAsync(RedmineIssue issue);
    Task<List<RedmineIssue>> GetIssuesAsync(string? projectId = null, int? userId = null);
    Task<List<RedmineIssue>> GetIssuesByUserAsync(string? city, string? department);
    Task<string> UploadFileAsync(byte[] fileContent, string filename);
    Task<List<RedmineCustomField>> GetCustomFieldsAsync(string projectId);
    Task<List<RedmineProject>> GetProjectsAsync();
    Task<RedmineProject?> GetProjectAsync(string identifier);
    Task<List<RedmineIssueCategory>> GetIssueCategoriesAsync(string projectId);
    Task<bool> AddUserToGroupAsync(int userId, int groupId);
    Task<int?> FindGroupAsync(string groupName);
    Task<int> CreateGroupAsync(string groupName, int userId);
    Task<List<RedmineUser>> GetAllUsersAsync();
    Task<string> GetOrgActiveAsync(string city);
}