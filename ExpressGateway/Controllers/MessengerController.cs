using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ExpressGateway.Services;
using ExpressGateway.Services.Redmine;
using ExpressGateway.Models;
using System.Text.Json;

namespace ExpressGateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessengerController : ControllerBase
{
    private readonly IExpressService _expressService;
    private readonly IRedmineBotService _redmineBotService; 
    private readonly ILogger<MessengerController> _logger;

    public MessengerController(
        IExpressService expressService,
        IRedmineBotService redmineBotService, 
        ILogger<MessengerController> logger)
    {
        _expressService = expressService;
        _redmineBotService = redmineBotService;
        _logger = logger;
    }
    
    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] SendMessageRequest request)
    {
        try
        {
            var result = await _expressService.SendMessageAsync(request.ChatId, request.Message, request.Asset);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("send-default")]
    public async Task<IActionResult> SendDefault([FromBody] SendDefaultMessageRequest request)
    {
        try
        {
            var result = await _expressService.SendToDefaultGroupAsync(request.Message);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending default message");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("ping")]
    public async Task<IActionResult> Ping()
    {
        try
        {
            var result = await _expressService.PingAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ping");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("messages/{chatId}")]
    public async Task<IActionResult> GetMessages(string chatId, [FromQuery] int limit = 50, [FromQuery] int offset = 0)
    {
        try
        {
            var messages = await _expressService.GetMessagesAsync(chatId, limit, offset);
            return Ok(new
            {
                success = true,
                chatId = chatId,
                count = messages.Count,
                messages = messages
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting messages");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("messages/new/{chatId}")]
    public async Task<IActionResult> GetNewMessages(string chatId, [FromQuery] DateTime? since)
    {
        try
        {
            var messages = await _expressService.GetNewMessagesAsync(chatId, since);
            return Ok(new
            {
                success = true,
                chatId = chatId,
                count = messages.Count,
                messages = messages
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting new messages");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("health")]
    public async Task<IActionResult> Health()
    {
        _logger.LogInformation($"{DateTime.UtcNow} SEND HEALTH STATUS");
        return Ok("OK");
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus([FromHeader(Name = "Authorization")] string? authorization = null)
    {
        try
        {
            _logger.LogInformation($"{DateTime.UtcNow} Start Get Status");
            _logger.LogInformation($"{DateTime.UtcNow} {Request.QueryString.Value}");
            _logger.LogInformation($"{DateTime.UtcNow} {string.Join(";", Request.Headers.Select(h => $"{h.Key}:{h.Value}"))}");

            var reqParams = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
            var headers = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(authorization))
            {
                headers["authorization"] = authorization;
            }

            var result = await GetBotChatAsync(
                reqParams.GetValueOrDefault("user_huid", ""),
                headers
            );

            _logger.LogInformation($"{DateTime.UtcNow} End Get Status {result}");
            return new JsonResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError($"{DateTime.UtcNow} ERROR Get Status {ex}");
            Response.StatusCode = 500;
            return new JsonResult(new { error = ex.Message });
        }
    }

    [HttpPost("command")]
    public async Task<IActionResult> SendCommand([FromBody] JsonElement requestBody)
    {
        try
        {
            _logger.LogInformation($"{DateTime.UtcNow} Start Send Command");
            
            var command = requestBody.GetProperty("command").GetString();
            var senderName = requestBody.TryGetProperty("sender", out var sender) 
                ? sender.GetString() ?? "User" 
                : "User";
            var chatId = requestBody.TryGetProperty("chat_id", out var chat) 
                ? chat.GetString() 
                : null;

            var result = await _redmineBotService.ProcessMessageAsync(command!, senderName, chatId);
            
            _logger.LogInformation($"{DateTime.UtcNow} End Send Command {result}");
            return Accepted(new { result = "accepted", response = result });
        }
        catch (Exception ex)
        {
            _logger.LogError($"{DateTime.UtcNow} ERROR Send Command {ex}");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    private async Task<string> GetBotChatAsync(string userHuid, Dictionary<string, string> headers)
    {
        try
        {
            _logger.LogInformation($"{DateTime.UtcNow} Getting bot chat for user: {userHuid}");

            using var client = new HttpClient();
            
            var jwt = headers.GetValueOrDefault("authorization", "");
            if (jwt.StartsWith("Bearer "))
            {
                jwt = jwt.Substring(7);
            }

            var url = $"https://x.ar-management.ru/api/v1/botx/chats/personal?user_huid={userHuid}";
            
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Authorization", $"Bearer {jwt}");

            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            _logger.LogInformation($"{DateTime.UtcNow} Bot chat response: {content}");
            
            return content;
        }
        catch (Exception ex)
        {
            _logger.LogError($"{DateTime.UtcNow} ERROR GetBotChat: {ex}");
            throw;
        }
    }
}