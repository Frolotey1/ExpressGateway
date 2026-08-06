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

            var reqParams = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
            var userHuid = reqParams.GetValueOrDefault("user_huid", "");

            if (string.IsNullOrEmpty(userHuid))
            {
                return BadRequest(new { error = "user_huid is required" });
            }

            var result = await _expressService.GetBotChatAsync(userHuid);

            _logger.LogInformation($"{DateTime.UtcNow} End Get Status");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError($"{DateTime.UtcNow} ERROR Get Status {ex}");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("command")]
    public async Task<IActionResult> SendCommand([FromBody] CommandRequest request)
    {
        try
        {
            _logger.LogInformation($"{DateTime.UtcNow} Start Send Command");
            
            if (request == null || string.IsNullOrEmpty(request.Command))
            {
                return BadRequest(new { error = "Command is required" });
            }

            var senderName = request.Sender ?? "User";
            
            _logger.LogInformation($"Command: {request.Command}, Sender: {senderName}");

            var result = await _redmineBotService.ProcessMessageAsync(request.Command, senderName);
            
            _logger.LogInformation($"{DateTime.UtcNow} End Send Command");
            return Accepted(new { result = "accepted", response = result });
        }
        catch (Exception ex)
        {
            _logger.LogError($"{DateTime.UtcNow} ERROR Send Command: {ex.Message}");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
