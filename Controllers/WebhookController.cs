using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using ExpressGateway.Models;
using ExpressGateway.Services;
using ExpressGateway.Services.Redmine;

namespace ExpressGateway.Controllers;

[ApiController]
[Route("api/webhook")]
public class WebhookController : ControllerBase
{
    private readonly ILogger<WebhookController> _logger;
    private readonly IRedmineBotService _redmineBotService;
    private readonly IExpressService _expressService;

    public WebhookController(
        ILogger<WebhookController> logger,
        IRedmineBotService redmineBotService,
        IExpressService expressService)
    {
        _logger = logger;
        _redmineBotService = redmineBotService;
        _expressService = expressService;
    }

    [HttpPost("express")]
    public async Task<IActionResult> HandleExpressWebhook([FromBody] JsonElement payload)
    {
        var requestId = Guid.NewGuid().ToString();
        try
        {
            _logger.LogInformation("[{RequestId}] Webhook received", requestId);
            
            var userHuid = payload.TryGetProperty("user_huid", out var huid) 
                ? huid.GetString() 
                : null;
            
            var chatId = payload.TryGetProperty("chat_id", out var chat) 
                ? chat.GetString() 
                : null;
            
            var messageText = payload.TryGetProperty("text", out var text) 
                ? text.GetString() 
                : null;

            if (string.IsNullOrEmpty(userHuid))
            {
                _logger.LogWarning("[{RequestId}] No user_huid in webhook", requestId);
                return BadRequest(new { error = "user_huid is required" });
            }

            if (string.IsNullOrEmpty(messageText))
            {
                return Ok(new { status = "ok", message = "Empty message ignored" });
            }

            if (string.IsNullOrEmpty(chatId))
            {
                _logger.LogInformation("[{RequestId}] chat_id not in webhook, getting from Express for user: {UserHuid}", 
                    requestId, userHuid);
                    
                
                var chatInfo = await _expressService.GetBotChatAsync(userHuid);
                
                var chatData = JsonSerializer.Deserialize<JsonElement>(chatInfo);
                if (chatData.TryGetProperty("result", out var result) && 
                    result.TryGetProperty("group_chat_id", out var chatIdElement))
                {
                    chatId = chatIdElement.GetString();
                    _logger.LogInformation("[{RequestId}] Found chat_id: {ChatId} for user: {UserHuid}", 
                        requestId, chatId, userHuid);
                }
                else
                {
                    _logger.LogError("[{RequestId}] Failed to get chat_id for user: {UserHuid}", 
                        requestId, userHuid);
                    return Ok(new { status = "error", message = "Cannot find chat for user" });
                }
            }

            var response = await _redmineBotService.ProcessMessageAsync(messageText, userHuid);
            
            if (!string.IsNullOrEmpty(response) && !string.IsNullOrEmpty(chatId))
            {
                await _expressService.SendMessageAsync(chatId, response);
                _logger.LogInformation("[{RequestId}] Response sent to chat: {ChatId}", requestId, chatId);
            }

            return Ok(new { 
                status = "ok", 
                received = true, 
                requestId,
                user_huid = userHuid,
                chat_id = chatId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{RequestId}] Error processing webhook", requestId);
            return Ok(new { status = "error", message = ex.Message, requestId });
        }
    }

    [HttpPost("test")]
    public async Task<IActionResult> TestWebhook([FromBody] TestWebhookRequest request)
    {
        _logger.LogInformation("Test webhook received: {Command} from {Sender}", 
            request.Command, request.Sender);

        var response = await _redmineBotService.ProcessMessageAsync(
            request.Command, 
            request.Sender ?? "test-user"
        );

        return Ok(new { 
            status = "ok", 
            response = response,
            request = request 
        });
    }
}
