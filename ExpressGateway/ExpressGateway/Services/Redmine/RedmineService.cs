using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using ExpressGateway.Services.Redmine.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class RedmineService : IRedmineService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RedmineService> _logger;
    private readonly string _baseUrl;
    private readonly string _apiToken;

    public RedmineService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<RedmineService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        var redmineSettings = configuration.GetSection("RedmineSettings");
        _baseUrl = redmineSettings["BaseUrl"] ?? throw new Exception("Redmine BaseUrl not configured");
        _apiToken = redmineSettings["ApiToken"] ?? throw new Exception("Redmine ApiToken not configured");

        _httpClient.BaseAddress = new Uri(_baseUrl);
        _httpClient.DefaultRequestHeaders.Add("X-Redmine-API-Key", _apiToken);
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<RedmineUser?> GetUserByApiKeyAsync(string apiKey)
    {
        try
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Redmine-API-Key", apiKey);
            
            var response = await client.GetAsync($"{_baseUrl}/users/current.json");
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<RedmineApiResponse<RedmineUser>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return data?.User;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user by API key");
            return null;
        }
    }

    public async Task<RedmineUser?> GetUserByFullNameAsync(string firstName, string lastName, string? city = null)
    {
        try
        {
            var url = $"/users.json?name={Uri.EscapeDataString($"{lastName} {firstName}")}";
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<RedmineUsersResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (data?.Users == null || !data.Users.Any())
                return null;

            if (!string.IsNullOrEmpty(city))
            {
                return data.Users.FirstOrDefault(u => 
                    u.City != null && u.City.Equals(city, StringComparison.OrdinalIgnoreCase));
            }

            return data.Users.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user by full name");
            return null;
        }
    }

    public async Task<RedmineUser> CreateUserAsync(RedmineUser user)
    {
        var payload = new
        {
            user = new
            {
                login = user.Login,
                firstname = user.FirstName,
                lastname = user.LastName,
                mail = user.Email,
                city = user.City,
                mobile = user.Mobile,
                landline_phone = user.LandlinePhone,
                department = user.Department,
                position = user.Position,
                org_active = user.OrgActive,
                auth_source_id = 1,
                update_from_login = true,
                generate_password = false
            }
        };

        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.PostAsync("/users.json", content);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to create user: {Response}", json);
            throw new Exception($"Failed to create user: {json}");
        }

        var data = JsonSerializer.Deserialize<RedmineApiResponse<RedmineUser>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return data?.User ?? throw new Exception("User creation failed");
    }

    public async Task<List<RedmineUser>> GetAllUsersAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/users.json?limit=100");
            if (!response.IsSuccessStatusCode)
                return new List<RedmineUser>();

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<RedmineUsersResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return data?.Users ?? new List<RedmineUser>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all users");
            return new List<RedmineUser>();
        }
    }

    public async Task<RedmineIssue?> GetIssueAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/issues/{id}.json?include=attachments,custom_fields");
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<RedmineApiResponse<RedmineIssue>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return data?.Issue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get issue {Id}", id);
            return null;
        }
    }

    public async Task<RedmineIssue> CreateIssueAsync(RedmineIssue issue)
    {
        try
        {
            var xml = new XDocument(
                new XElement("issue",
                    new XElement("project_id", issue.Project?.Identifier ?? issue.Project?.Name),
                    issue.Tracker != null ? new XElement("tracker_id", issue.Tracker.Id) : null,
                    issue.Status != null ? new XElement("status_id", issue.Status.Id) : null,
                    issue.Priority != null ? new XElement("priority_id", issue.Priority.Id) : null,
                    issue.CategoryId.HasValue ? new XElement("category_id", issue.CategoryId.Value) : null,
                    new XElement("subject", issue.Subject ?? "Без темы"),
                    new XElement("description", issue.Description ?? "Без описания"),
                    issue.AssignedTo != null ? new XElement("assigned_to_id", issue.AssignedTo.Id) : null,
                    issue.CustomFields.Any() ? 
                        new XElement("custom_fields", 
                            issue.CustomFields.Select(cf => 
                                new XElement("custom_field", 
                                    new XAttribute("id", cf.Id),
                                    new XElement("value", cf.Value ?? "")
                                )
                            )
                        ) : null,
                    issue.Attachments.Any() ? 
                        new XElement("uploads",
                            issue.Attachments.Select(a => 
                                new XElement("upload", 
                                    new XElement("token", a.Token ?? "")
                                )
                            )
                        ) : null
                )
            );

            var content = new StringContent(xml.ToString(), Encoding.UTF8, "application/xml");
            content.Headers.Add("X-Redmine-API-Key", _apiToken);

            var response = await _httpClient.PostAsync("/issues.xml", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to create issue: {Response}", responseBody);
                throw new Exception($"Failed to create issue: {responseBody}");
            }

            var doc = XDocument.Parse(responseBody);
            var issueElement = doc.Root;
            
            return new RedmineIssue
            {
                Id = int.Parse(issueElement?.Element("id")?.Value ?? "0"),
                Subject = issueElement?.Element("subject")?.Value ?? issue.Subject!,
                Description = issueElement?.Element("description")?.Value ?? issue.Description!,
                CategoryId = issueElement?.Element("category_id") != null ? 
                    int.Parse(issueElement.Element("category_id")?.Value ?? "0") : null,
                Status = new RedmineStatus
                {
                    Id = int.Parse(issueElement?.Element("status")?.Element("id")?.Value ?? "0"),
                    Name = issueElement?.Element("status")?.Element("name")?.Value ?? "Новая"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create issue");
            throw;
        }
    }

    public async Task<List<RedmineIssue>> GetIssuesAsync(string? projectId = null, int? userId = null)
    {
        try
        {
            var url = "/issues.json?limit=100";
            if (!string.IsNullOrEmpty(projectId))
                url += $"&project_id={projectId}";
            if (userId.HasValue)
                url += $"&assigned_to_id={userId}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return new List<RedmineIssue>();

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<RedmineIssuesResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return data?.Issues ?? new List<RedmineIssue>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get issues");
            return new List<RedmineIssue>();
        }
    }

    public async Task<List<RedmineIssue>> GetIssuesByUserAsync(string? city, string? department)
    {
        return await GetIssuesAsync();
    }
    public async Task<List<RedmineProject>> GetProjectsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/projects.json?limit=100");
            if (!response.IsSuccessStatusCode)
                return new List<RedmineProject>();

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<RedmineProjectsResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return data?.Projects ?? new List<RedmineProject>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get projects");
            return new List<RedmineProject>();
        }
    }
    public async Task<RedmineProject?> GetProjectAsync(string identifier)
    {
        var projects = await GetProjectsAsync();
        return projects.FirstOrDefault(p => 
            p.Identifier?.Equals(identifier, StringComparison.OrdinalIgnoreCase) == true);
    }    
    public async Task<List<RedmineIssueCategory>> GetIssueCategoriesAsync(string projectId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/projects/{projectId}/issue_categories.json");
            if (!response.IsSuccessStatusCode)
                return new List<RedmineIssueCategory>();

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<RedmineCategoriesResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return data?.IssueCategories ?? new List<RedmineIssueCategory>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get issue categories for project {ProjectId}", projectId);
            return new List<RedmineIssueCategory>();
        }
    }

    public async Task<List<RedmineCustomField>> GetCustomFieldsAsync(string projectId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/projects/{projectId}.json?include=issue_custom_fields");
            if (!response.IsSuccessStatusCode)
                return new List<RedmineCustomField>();

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<RedmineProjectResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return data?.Project?.IssueCustomFields ?? new List<RedmineCustomField>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get custom fields for project {ProjectId}", projectId);
            return new List<RedmineCustomField>();
        }
    }

    public async Task<List<int>> GetRequiredCustomFieldIdsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/custom_fields.json");
            if (!response.IsSuccessStatusCode)
                return new List<int>();

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<RedmineCustomFieldsResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return data?.CustomFields?
                .Where(cf => cf.IsRequired)
                .Select(cf => cf.Id)
                .ToList() ?? new List<int>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get required custom fields");
            return new List<int>();
        }
    }

    public async Task<string> UploadFileAsync(byte[] fileContent, string filename)
    {
        try
        {
            using var content = new ByteArrayContent(fileContent);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            var response = await _httpClient.PostAsync($"/uploads.json?filename={Uri.EscapeDataString(filename)}", content);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to upload file: {json}");

            var data = JsonSerializer.Deserialize<RedmineUploadResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return data?.Upload?.Token ?? throw new Exception("No token in response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file");
            throw;
        }
    }

    public async Task<bool> AddUserToGroupAsync(int userId, int groupId)
    {
        try
        {
            var payload = new { user_id = userId };
            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync($"/groups/{groupId}/users.json", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add user to group");
            return false;
        }
    }

    public async Task<int?> FindGroupAsync(string groupName)
    {
        try
        {
            var response = await _httpClient.GetAsync("/groups.json");
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<RedmineGroupsResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return data?.Groups?.FirstOrDefault(g => 
                g.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase))?.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to find group");
            return null;
        }
    }

    public async Task<int> CreateGroupAsync(string groupName, int userId)
    {
        try
        {
            var payload = new
            {
                group = new
                {
                    name = groupName,
                    user_ids = new[] { userId }
                }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync("/groups.json", content);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to create group: {json}");

            var data = JsonSerializer.Deserialize<RedmineGroupResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return data?.Group?.Id ?? throw new Exception("Group creation failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create group");
            throw;
        }
    }
    public async Task<string> GetOrgActiveAsync(string city)
    {
        try
        {
            var response = await _httpClient.GetAsync("/org_actives.json");
            if (!response.IsSuccessStatusCode)
                return "org_active";

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<RedmineOrgActivesResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return data?.OrgActives?.FirstOrDefault(o => 
                o.City.Equals(city, StringComparison.OrdinalIgnoreCase))?.Name 
                   ?? "org_active";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get org active");
            return "org_active";
        }
    }
}