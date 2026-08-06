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
                
            var senderName = "Пользователь";
            if (payload.TryGetProperty("sender", out var sender))
            {
                senderName = sender.TryGetProperty("name", out var name) 
                    ? name.GetString() ?? "Пользователь" 
                    : "Пользователь";
            }

            _logger.LogInformation(
                "[{RequestId}] Автоматический user_huid: {UserHuid}, 📱 chat_id: {ChatId}, Команда: {Text}", 
                requestId, userHuid ?? "null", chatId ?? "null", messageText ?? "null");

            if (string.IsNullOrEmpty(messageText))
            {
                return Ok(new { status = "ok", message = "Empty message ignored" });
            }

            var senderId = userHuid ?? senderName;

            var response = await _redmineBotService.ProcessMessageAsync(messageText, senderId);
                
            _logger.LogInformation("[{RequestId}] Ответ бота: {Response}", requestId, response);

            if (!string.IsNullOrEmpty(response) && !string.IsNullOrEmpty(chatId))
            {
                var sendResult = await _expressService.SendMessageAsync(chatId, response);
                    
                if (sendResult.Success)
                {
                    _logger.LogInformation("[{RequestId}] Ответ отправлен в чат: {ChatId}", requestId, chatId);
                }
                else
                {
                    _logger.LogError("[{RequestId}] Ошибка отправки: {Error}", requestId, sendResult.Error);
                }
            }
            else
            {
                _logger.LogWarning("[{RequestId}] Нет ответа или chat_id", requestId);
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
            _logger.LogError(ex, "[{RequestId}] Ошибка webhook", requestId);
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
