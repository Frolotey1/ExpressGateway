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
            _logger.LogDebug("[{RequestId}] Payload: {Payload}", requestId, payload);

            var userHuid = payload.TryGetProperty("user_huid", out var huid) 
                ? huid.GetString() 
                : null;
            
            var messageText = payload.TryGetProperty("text", out var text) 
                ? text.GetString() 
                : null;
            
            var chatId = payload.TryGetProperty("chat_id", out var chat) 
                ? chat.GetString() 
                : null;

            var senderName = "Пользователь";
            if (payload.TryGetProperty("sender", out var sender))
            {
                senderName = sender.TryGetProperty("name", out var name) 
                    ? name.GetString() ?? "Пользователь" 
                    : "Пользователь";
            }

            _logger.LogInformation(
                "[{RequestId}] User: {UserHuid}, Chat: {ChatId}, Command: {Text}", 
                requestId, userHuid ?? "null", chatId ?? "null", messageText ?? "null");

            if (string.IsNullOrEmpty(messageText))
            {
                _logger.LogWarning("[{RequestId}] Empty message received", requestId);
                return Ok(new { status = "ok", message = "Empty message ignored" });
            }

            var senderId = userHuid ?? "anonymous";

            var response = await _redmineBotService.ProcessMessageAsync(messageText, senderId);
            
            if (!string.IsNullOrEmpty(response) && !string.IsNullOrEmpty(chatId))
            {
                await _expressService.SendMessageAsync(chatId, response);
                _logger.LogInformation("[{RequestId}] Response sent: {Response}", requestId, response);
            }
            else
            {
                _logger.LogWarning("[{RequestId}] No response or chatId - message not sent", requestId);
            }

            return Ok(new { status = "ok", received = true, requestId });
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
