using Microsoft.AspNetCore.Mvc;
using ExpressGateway.Services;
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

    [HttpGet("health")]
    public IActionResult Health()
    {
        _logger.LogInformation("Health check requested");
        return Ok(new { status = "OK", timestamp = DateTime.UtcNow });
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus([FromHeader(Name = "Authorization")] string? authorization = null)
    {
        try
        {
            _logger.LogInformation("GetStatus called");

            var reqParams = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
            var userHuid = reqParams.GetValueOrDefault("user_huid", "");

            if (string.IsNullOrEmpty(userHuid))
            {
                return BadRequest(new { error = "user_huid is required" });
            }

            _logger.LogInformation($"Getting bot chat for user: {userHuid}");

            var chatInfo = await _expressService.GetBotChatAsync(userHuid);
            
            _logger.LogInformation($"Raw chatInfo response: {chatInfo}");

            string? chatId = null;
            string? chatName = null;
            List<string>? members = null;
            bool isSuccess = false;

            try
            {
                var chatData = JsonSerializer.Deserialize<JsonElement>(chatInfo);
                
                if (chatData.TryGetProperty("status", out var statusElement))
                {
                    var statusValue = statusElement.GetString();
                    isSuccess = statusValue == "ok";
                    _logger.LogInformation($"Status from Express: {statusValue}");
                }

                if (chatData.TryGetProperty("result", out var resultElement))
                {
                    _logger.LogInformation("Result found in response");
                    
                    if (resultElement.TryGetProperty("group_chat_id", out var chatIdElement))
                    {
                        chatId = chatIdElement.GetString();
                        _logger.LogInformation($"ChatId found: {chatId}");
                    }
                    
                    if (resultElement.TryGetProperty("name", out var nameElement))
                    {
                        chatName = nameElement.GetString();
                        _logger.LogInformation($"Chat name: {chatName}");
                    }
                    
                    if (resultElement.TryGetProperty("members", out var memberArray))
                    {
                        members = memberArray.EnumerateArray()
                            .Select(m => m.GetString())
                            .Where(m => m != null)
                            .Cast<string>()
                            .ToList();
                        _logger.LogInformation($"Members count: {members?.Count ?? 0}");
                    }
                }
                else
                {
                    _logger.LogWarning("No 'result' property in response, trying to parse as is");
                    
                    if (chatData.TryGetProperty("group_chat_id", out var directChatId))
                    {
                        chatId = directChatId.GetString();
                        _logger.LogInformation($"ChatId found directly: {chatId}");
                    }
                    
                    if (chatData.TryGetProperty("name", out var directName))
                    {
                        chatName = directName.GetString();
                    }
                }
            }
            catch (Exception parseEx)
            {
                _logger.LogError($"Error parsing chat info: {parseEx.Message}");
                _logger.LogError($"Raw response: {chatInfo}");
            }

            var pingResult = await _expressService.PingAsync();
            
            var commands = await _redmineBotService.GetAvailableCommandsAsync();

            var result = new
            {
                status = isSuccess ? "ok" : "error",
                result = new
                {
                    enabled = isSuccess,
                    status_message = isSuccess ? "it's work!" : "service unavailable",
                    chat_info = new
                    {
                        chat_id = chatId,
                        chat_name = chatName,
                        members = members ?? new List<string>(),
                        user_huid = userHuid
                    },
                    commands = commands.Select(c => new
                    {
                        description = c.Description,
                        name = c.Name,
                        body = c.Body
                    }).ToList()
                }
            };

            _logger.LogInformation($"GetStatus completed. ChatId: {chatId}, Commands: {commands.Count}");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError($"GetStatus error: {ex.Message}");
            _logger.LogError(ex.StackTrace);
            
            return StatusCode(500, new 
            { 
                status = "error", 
                error = ex.Message,
                result = new
                {
                    enabled = false,
                    status_message = "error",
                    chat_info = (object?)null,
                    commands = new List<object>()
                }
            });
        }
    }

    [HttpPost("command")]
    public async Task<IActionResult> SendCommand([FromBody] JsonElement requestBody)
    {
        try
        {
            _logger.LogInformation($"SendCommand called");
            _logger.LogDebug($"Raw request: {requestBody.GetRawText()}");

            string? commandText = null;
            string? senderName = "User";
            string? syncId = null;
            string? commandType = null;

            if (requestBody.TryGetProperty("command", out var commandElement))
            {
                if (commandElement.ValueKind == JsonValueKind.String)
                {
                    commandText = commandElement.GetString();
                    _logger.LogInformation($"Simple command format: {commandText}");
                }
                else if (commandElement.ValueKind == JsonValueKind.Object)
                {
                    if (commandElement.TryGetProperty("body", out var bodyElement))
                    {
                        commandText = bodyElement.GetString();
                        _logger.LogInformation($"Express command format: {commandText}");
                    }
                    
                    if (commandElement.TryGetProperty("command_type", out var typeElement))
                    {
                        commandType = typeElement.GetString();
                    }

                    if (commandElement.TryGetProperty("data", out var dataElement))
                    {
                        _logger.LogDebug($"Command data: {dataElement.GetRawText()}");
                    }
                }
            }

            if (requestBody.TryGetProperty("sender", out var senderElement))
            {
                senderName = senderElement.GetString() ?? "User";
            }

            if (requestBody.TryGetProperty("sync_id", out var syncElement))
            {
                syncId = syncElement.GetString();
            }

            if (string.IsNullOrEmpty(commandText))
            {
                _logger.LogWarning($"No command found in request");
                return BadRequest(new 
                { 
                    status = "error",
                    error = "Command is required",
                    hint = "Simple format: { 'command': '/help' } or Express format: { 'command': { 'body': '/help' } }"
                });
            }

            _logger.LogInformation($"Command: {commandText}, Type: {commandType ?? "unknown"}, Sender: {senderName}, SyncId: {syncId ?? "null"}");

            var response = await _redmineBotService.ProcessMessageAsync(commandText, senderName);

            _logger.LogInformation($"Command processed successfully");

            return Ok(new
            {
                status = "ok",
                message = "Команда получена и обработана",
                result = new
                {
                    command = commandText,
                    sender = senderName,
                    response = response,       
                    sync_id = syncId,
                    command_type = commandType,
                    feedback = new
                    {
                        received = true,
                        received_at = DateTime.UtcNow,
                        processed = true,
                        processed_at = DateTime.UtcNow
                    }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"SendCommand error: {ex.Message}");
            _logger.LogError($"Raw request: {requestBody.GetRawText()}");
            
            return StatusCode(500, new 
            { 
                status = "error", 
                message = "Ошибка при обработке команды",
                error = ex.Message
            });
        }
    }
}