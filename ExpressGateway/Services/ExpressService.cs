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
    private readonly string _chatId;
    private readonly HttpClient _httpClient;
    private string? _jwtToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

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
        _chatId = expressSettings["ChatId"] 
            ?? throw new Exception("ChatId not configured");

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
            if (!string.IsNullOrEmpty(_jwtToken) && DateTime.UtcNow < _tokenExpiry)
            {
                return _jwtToken;
            }

            var signature = GenerateSignature(_botId, _secKey);
            var url = $"/api/v2/botx/bots/{_botId}/token?signature={signature}";

            _logger.LogInformation("Getting JWT token from: {Url}", url);

            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to get JWT token: {response.StatusCode} - {content}");
            }

            var json = JsonSerializer.Deserialize<JwtResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (json?.Result == null || json.Status?.ToUpper() != "OK")
            {
                throw new Exception($"Invalid JWT response: {content}");
            }

            _jwtToken = json.Result;
            _tokenExpiry = DateTime.UtcNow.AddMinutes(55);

            _logger.LogInformation("JWT token obtained successfully");
            return _jwtToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get JWT token");
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
                chatId = chatId,
                text = message,
                botId = _botId
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            _logger.LogInformation("Sending payload: {Payload}", jsonPayload);

            var content = new StringContent(
                jsonPayload,
                Encoding.UTF8,
                "application/json"
            );

            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v4/messages");
            request.Content = content;
            request.Headers.Add("Authorization", $"Bearer {jwtToken}");

            var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Response Status: {StatusCode}", response.StatusCode);

            if (response.IsSuccessStatusCode)
            {
                return new SendMessageResponse
                {
                    Success = true,
                    ChatId = chatId,
                    SentAt = DateTime.UtcNow,
                    Status = "sent",
                    MessageId = Guid.NewGuid().ToString(),
                    ExpressResponse = "Message sent successfully"
                };
            }

            return new SendMessageResponse
            {
                Success = false,
                Error = $"API Error: {response.StatusCode} - {responseBody}",
                ChatId = chatId
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
        return await SendMessageAsync(_chatId, message);
    }
    public async Task<ChatListResponse> GetChatsAsync(int limit = 50, int offset = 0)
    {
        try
        {
            var jwtToken = await GetJwtTokenAsync();

            using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v4/chats?limit={limit}&offset={offset}");
            request.Headers.Add("Authorization", $"Bearer {jwtToken}");

            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var chats = JsonSerializer.Deserialize<ChatListResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return chats ?? new ChatListResponse();
            }

            _logger.LogWarning("Failed to get chats: {StatusCode}", response.StatusCode);
            return new ChatListResponse();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get chats");
            return new ChatListResponse();
        }
    }

    public async Task<PingResponse> PingAsync()
    {
        return new PingResponse
        {
            Status = "ok",
            Message = "pong"
        };
    }
    private class JwtResponse
    {
        public string? Status { get; set; }
        public string? Result { get; set; }
    }
}