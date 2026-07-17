using ExpressGateway.Core.Impl.Messenger;
using ExpressGateway.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ExpressGateway.Services;

public class ExpressService : IExpressService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ExpressService> _logger;
    private readonly Dictionary<string, ChatInfo> _chatCache = new();
    private readonly string _defaultChatId;
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl;
    private readonly string _apiKey;
    private string? _currentWebhookUrl;
    private string? _currentWebhookSecret;

    public ExpressService(
        IConfiguration configuration, 
        ILogger<ExpressService> logger,
        HttpClient httpClient)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
        
        _defaultChatId = _configuration["ExpressSettings:DefaultChatId"] 
            ?? throw new Exception("DefaultChatId not configured");
        
        _apiBaseUrl = _configuration["ExpressSettings:ApiUrl"] 
            ?? throw new Exception("ApiUrl not configured");
        
        _apiKey = _configuration["ExpressSettings:ApiKey"] 
            ?? throw new Exception("ApiKey not configured");
        
        _httpClient.DefaultRequestHeaders.Add("X-Api-Key", _apiKey);
        _httpClient.BaseAddress = new Uri(_apiBaseUrl);
        
        LoadChats();
        LoadWebhookConfig();
    }

    private void LoadChats()
    {
        var section = _configuration.GetSection("ExpressSettings:Chats");
        var groups = section.GetChildren();

        foreach (var group in groups)
        {
            var asset = group.Key;
            var chatId = group.Value;
            
            _chatCache[asset.ToLower()] = new ChatInfo
            {
                Id = chatId,
                Asset = asset,
                ChatId = chatId,
                Name = $"Chat for {asset}",
                IsDefault = chatId == _defaultChatId,
                MembersCount = 0
            };
        }

        if (!_chatCache.ContainsKey("default"))
        {
            _chatCache["default"] = new ChatInfo
            {
                Id = _defaultChatId,
                Asset = "default",
                ChatId = _defaultChatId,
                Name = "Default Group",
                IsDefault = true,
                MembersCount = 0
            };
        }
    }

    private void LoadWebhookConfig()
    {
        _currentWebhookUrl = _configuration["ExpressSettings:WebhookUrl"];
        _currentWebhookSecret = _configuration["ExpressSettings:WebhookSecret"];
    }

    public async Task<SendMessageResponse> SendMessageAsync(string chatId, string message, string? asset = null)
    {
        try
        {
            _logger.LogInformation("Sending to Express: ChatId={ChatId}, Asset={Asset}", chatId, asset);

            var botId = _configuration["ExpressSettings:BotId"]
                ?? throw new Exception("BotId not configured");

            var messenger = new ExpressMessenger(chatId, botId);
            var result = messenger.Send(asset ?? "", message);

            return new SendMessageResponse
            {
                Success = true,
                ChatId = chatId,
                SentAt = DateTime.UtcNow,
                Status = "sent",
                MessageId = Guid.NewGuid().ToString(),
                ExpressResponse = result
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message");
            return new SendMessageResponse
            {
                Success = false,
                Error = ex.Message,
                ChatId = chatId
            };
        }
    }

    public async Task<SendMessageResponse> SendToDefaultGroupAsync(string message)
    {
        return await SendMessageAsync(_defaultChatId, message);
    }

    public async Task<ChatListResponse> GetChatsAsync(int limit = 50, int offset = 0)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/chats?limit={limit}&offset={offset}");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var externalChats = JsonSerializer.Deserialize<ChatListResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (externalChats != null && externalChats.Chats.Any())
                {
                    return externalChats;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get chats from external API, using cache");
        }

        var chats = _chatCache.Values
            .Skip(offset)
            .Take(limit)
            .Select(c => new ChatInfo
            {
                Id = c.ChatId,
                Name = c.Name ?? c.Asset,
                MembersCount = c.MembersCount
            })
            .ToList();

        return new ChatListResponse
        {
            Chats = chats,
            Total = _chatCache.Count
        };
    }

    public async Task<WebhookResponse> SetWebhookAsync(WebhookRequest request)
    {
        try
        {
            _logger.LogInformation("Setting webhook: {Url}", request.Url);

            if (!Uri.IsWellFormedUriString(request.Url, UriKind.Absolute))
            {
                return new WebhookResponse
                {
                    Status = "error",
                    Message = "Invalid URL format"
                };
            }

            _currentWebhookUrl = request.Url;
            _currentWebhookSecret = request.Secret;

            var webhookPayload = new
            {
                url = request.Url,
                secret = request.Secret,
                events = new[] { "message", "chat", "user" }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(webhookPayload),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("/api/webhook/set", content);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<WebhookResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result ?? new WebhookResponse
                {
                    Status = "ok",
                    Message = "Webhook set successfully",
                    WebhookUrl = request.Url
                };
            }

            return new WebhookResponse
            {
                Status = "ok",
                Message = "Webhook configured locally",
                WebhookUrl = request.Url
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set webhook");
            return new WebhookResponse
            {
                Status = "error",
                Message = $"Failed to set webhook: {ex.Message}"
            };
        }
    }

    public async Task<WebhookResponse> GetWebhookAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/webhook/current");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<WebhookResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result != null)
                {
                    return result;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get webhook from external API");
        }

        return new WebhookResponse
        {
            Status = _currentWebhookUrl != null ? "configured" : "not_configured",
            WebhookUrl = _currentWebhookUrl!,
            Message = _currentWebhookUrl != null ? "Webhook is configured" : "No webhook configured"
        };
    }

    public async Task<bool> DeleteWebhookAsync()
    {
        try
        {
            var response = await _httpClient.DeleteAsync("/api/webhook/delete");

            if (response.IsSuccessStatusCode)
            {
                _currentWebhookUrl = null;
                _currentWebhookSecret = null;
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete webhook");
        }

        _currentWebhookUrl = null;
        _currentWebhookSecret = null;
        return true;
    }

    public async Task<bool> ProcessWebhookEventAsync(WebhookEvent eventData)
    {
        try
        {
            _logger.LogInformation("Processing webhook event: {EventType}", eventData.EventType);
            _logger.LogDebug("Event data: {@EventData}", eventData);

            if (string.IsNullOrEmpty(eventData.EventType))
            {
                _logger.LogWarning("Event type is empty");
                return false;
            }

            switch (eventData.EventType?.ToLower())
            {
                case "message":
                    _logger.LogInformation("Message event received: {MessageId}, ChatId: {ChatId}", 
                        eventData.MessageId, eventData.ChatId);
                    
                    if (eventData.Message != null)
                    {
                        _logger.LogInformation("Message text: {Text}", eventData.Message.Text);
                    }
                    break;

                case "chat":
                    _logger.LogInformation("Chat event received: {ChatId}", eventData.ChatId);
                    
                    if (eventData.ChatInfo != null)
                    {
                        _logger.LogInformation("Chat name: {ChatName}", eventData.ChatInfo.Name);
                    }
                    break;

                case "user":
                    _logger.LogInformation("User event received: {UserId}", eventData.UserId);
                    
                    if (eventData.UserInfo != null)
                    {
                        _logger.LogInformation("User name: {UserName}", eventData.UserInfo.Name);
                    }
                    break;

                default:
                    _logger.LogWarning("Unknown webhook event type: {EventType}", eventData.EventType);
                    return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process webhook event");
            return false;
        }
    }

    public async Task<PingResponse> PingAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/ping");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<PingResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result ?? new PingResponse
                {
                    Status = "ok",
                    Message = "pong"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "External API ping failed, using local response");
        }

        return new PingResponse
        {
            Status = "ok",
            Message = "pong (local)"
        };
    }
}