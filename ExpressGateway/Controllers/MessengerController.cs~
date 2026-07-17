using Microsoft.AspNetCore.Mvc;
using ExpressGateway.Models;
using ExpressGateway.Services;

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
    [ProducesResponseType(typeof(ApiResponse<SendMessageResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<IActionResult> Send([FromBody] SendMessageRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.ChatId))
                return BadRequest(new ApiResponse<object> { Success = false, Error = "ChatId is required" });

            if (string.IsNullOrEmpty(request.Message))
                return BadRequest(new ApiResponse<object> { Success = false, Error = "Message is required" });

            var result = await _expressService.SendMessageAsync(request.ChatId, request.Message, request.Asset);

            return Ok(new ApiResponse<SendMessageResponse>
            {
                Success = true,
                Data = result,
                Message = "Message sent successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message");
            return StatusCode(500, new ApiResponse<object> { Success = false, Error = ex.Message });
        }
    }

    [HttpPost("send-default")]
    [ProducesResponseType(typeof(ApiResponse<SendMessageResponse>), 200)]
    public async Task<IActionResult> SendToDefault([FromBody] string message)
    {
        if (string.IsNullOrEmpty(message))
            return BadRequest(new ApiResponse<object> { Success = false, Error = "Message is required" });

        var result = await _expressService.SendToDefaultGroupAsync(message);

        return Ok(new ApiResponse<SendMessageResponse>
        {
            Success = true,
            Data = result,
            Message = "Message sent to default group"
        });
    }

    [HttpGet("chats")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ChatInfo>>), 200)]
    public async Task<IActionResult> GetChats()
    {
        var chats = await _expressService.GetChatsAsync();
        return Ok(new ApiResponse<IEnumerable<ChatInfo>>
        {
            Success = true,
            Data = chats,
            Message = $"Found {chats.Count()} chats"
        });
    }

    [HttpPost("webhook")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> Webhook([FromBody] WebhookRequest request)
    {
        _logger.LogInformation("Webhook received: {Message}", request.Message);
        await _expressService.ProcessWebhookAsync(request);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Webhook processed",
            Data = new { received = request.Message, timestamp = DateTime.UtcNow }
        });
    }

    [HttpGet("ping")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public IActionResult Ping()
    {
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "pong",
            Data = new
            {
                timestamp = DateTime.UtcNow,
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "unknown"
            }
        });
    }
}
