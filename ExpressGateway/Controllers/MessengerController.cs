using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ExpressGateway.Services;
using ExpressGateway.Models;

namespace ExpressGateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessengerController : ControllerBase
{
    private readonly IExpressService _expressService;
    private readonly ILogger<MessengerController> _logger;

    public MessengerController(IExpressService expressService, ILogger<MessengerController> logger)
    {
        _expressService = expressService;
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

    [HttpGet("chats")]
    public async Task<IActionResult> GetChats([FromQuery] int limit = 50, [FromQuery] int offset = 0)
    {
        try
        {
            var result = await _expressService.GetChatsAsync(limit, offset);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chats");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> SetWebhook([FromBody] WebhookRequest request)
    {
        try
        {
            var result = await _expressService.SetWebhookAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting webhook");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("webhook")]
    public async Task<IActionResult> GetWebhook()
    {
        try
        {
            var result = await _expressService.GetWebhookAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting webhook");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpDelete("webhook")]
    public async Task<IActionResult> DeleteWebhook()
    {
        try
        {
            var result = await _expressService.DeleteWebhookAsync();
            return Ok(new { success = result, message = result ? "Webhook deleted" : "Failed to delete webhook" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting webhook");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("webhook/event")]
    public async Task<IActionResult> ProcessWebhookEvent([FromBody] WebhookEvent eventData)
    {
        try
        {
            var result = await _expressService.ProcessWebhookEventAsync(eventData);
            return Ok(new { success = result, message = result ? "Event processed" : "Failed to process event" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook event");
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
}