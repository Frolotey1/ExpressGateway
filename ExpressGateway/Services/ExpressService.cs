using ExpressGateway.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ExpressGateway.Services;

public class ExpressService : IExpressService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ExpressService> _logger;
    private readonly string _apiUrl;
    private readonly string _secKey;
    private readonly string _botId;
    private readonly HttpClient _httpClient;
    private string? _jwtToken;

    public ExpressService(
        IConfiguration configuration, 
        ILogger<ExpressService> logger,
        HttpClient httpClient)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;

        var expressSettings = _configuration.GetSection("ExpressSettings");
        
        _apiUrl = expressSettings["ApiUrl"] 
            ?? throw new Exception("ApiUrl not configured");
        _secKey = expressSettings["SecKey"] 
            ?? throw new Exception("SecKey not configured");
        _botId = expressSettings["BotId"] 
            ?? throw new Exception("BotId not configured");

        _httpClient.BaseAddress = new Uri(_apiUrl);
    }

    private string GenerateSignature(string botId, string secKey)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secKey);
        var dataBytes = Encoding.UTF8.GetBytes(botId);
        
        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(dataBytes);
        return BitConverter.ToString(hash).Replace("-", "").ToUpper();
    }

    private async Task<string> GetJwtTokenAsync()
    {
        try
        {
            if (!string.IsNullOrEmpty(_jwtToken))
            {
                _logger.LogDebug("Using existing JWT token");
                return _jwtToken;
            }

            _jwtToken = null;

            var signature = GenerateSignature(_botId, _secKey);
            var url = $"/api/v2/botx/bots/{_botId}/token?signature={signature}";

            _logger.LogInformation("Getting JWT token from: {Url}", url);

            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get JWT token. Status: {StatusCode}, Response: {Response}", 
                    response.StatusCode, content);
                throw new Exception($"Failed to get JWT token: {response.StatusCode} - {content}");
            }

            var json = JsonSerializer.Deserialize<JwtResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (json?.Result == null || json.Status?.ToUpper() != "OK")
            {
                _logger.LogError("Invalid JWT response: {Response}", content);
                throw new Exception($"Invalid JWT response: {content}");
            }

            _jwtToken = json.Result;
            _logger.LogInformation("JWT token obtained successfully");
            return _jwtToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get JWT token");
            _jwtToken = null; 
            throw;
        }
    }

    public async Task<string> GetBotChatAsync(string userHuid)
    {
        try
        {
            _logger.LogInformation("Getting bot chat for user: {UserHuid}", userHuid);
            
            var jwtToken = await GetJwtTokenAsync();
            
            var url = $"/api/v1/botx/chats/personal?user_huid={userHuid}";
            
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Authorization", $"Bearer {jwtToken}");
            
            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            
            _logger.LogInformation("Bot chat response: {Response}", content);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get bot chat: {StatusCode} - {Content}", 
                    response.StatusCode, content);
                return content;
            }
            
            return content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get bot chat for user: {UserHuid}", userHuid);
            throw;
        }
    }

    public async Task<SendMessageResponse> SendMessageAsync(string chatId, string message, string? asset = null)
    {
        try
        {
            var jwtToken = await GetJwtTokenAsync();

            var payload = new
            {
                group_chat_id = chatId,
                notification = new
                {
                    body = message
                }
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            _logger.LogInformation("Sending payload to Express: {Payload}", jsonPayload);

            var content = new StringContent(
                jsonPayload,
                Encoding.UTF8,
                "application/json"
            );

            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v4/botx/notifications/direct");
            request.Content = content;
            request.Headers.Add("Authorization", $"Bearer {jwtToken}");

            var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Response from Express: Status={StatusCode}, Body={ResponseBody}", 
                response.StatusCode, responseBody);

            if (response.IsSuccessStatusCode)
            {
                return new SendMessageResponse
                {
                    Success = true,
                    ChatId = chatId,
                    SentAt = DateTime.UtcNow,
                    Status = "sent",
                    MessageId = Guid.NewGuid().ToString(),
                    ExpressResponse = responseBody
                };
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _jwtToken = null;
                _logger.LogWarning("JWT token expired, will request new one next time");
            }

            return new SendMessageResponse
            {
                Success = false,
                Error = $"Express API Error: {response.StatusCode} - {responseBody}",
                ChatId = chatId,
                ExpressResponse = responseBody
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to chat: {ChatId}", chatId);
            return new SendMessageResponse
            {
                Success = false,
                Error = ex.Message,
                ChatId = chatId
            };
        }
    }

    public async Task<List<IncomingMessage>> GetNewMessagesAsync(string chatId, DateTime? since = null)
    {
        try
        {
            var jwtToken = await GetJwtTokenAsync();
            
            var sinceParam = since ?? DateTime.UtcNow.AddMinutes(-5);
            var url = $"/api/v4/botx/chats/{chatId}/messages";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Authorization", $"Bearer {jwtToken}");

            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get new messages: {StatusCode}", response.StatusCode);
                return new List<IncomingMessage>();
            }

            var result = JsonSerializer.Deserialize<MessagesResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result?.Messages ?? new List<IncomingMessage>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get new messages for chat {ChatId}", chatId);
            return new List<IncomingMessage>();
        }
    }
    public async Task<PingResponse> PingAsync()
    {
        try
        {
            await GetJwtTokenAsync();
            
            return new PingResponse
            {
                Status = "ok",
                Message = "pong"
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ping failed");
            return new PingResponse
            {
                Status = "error",
                Message = ex.Message
            };
        }
    }
}