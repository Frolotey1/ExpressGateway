using ExpressGateway.Models;
using ExpressGateway.Services.Redmine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ExpressGateway.Services;
public class MessageProcessorService
{
    private readonly ILogger<MessageProcessorService> _logger;
    private readonly IExpressService _expressService;
    private readonly IRedmineBotService _redmineBotService;
    private readonly IConfiguration _configuration;

    public MessageProcessorService(
        ILogger<MessageProcessorService> logger,
        IExpressService expressService,
        IRedmineBotService redmineBotService,
        IConfiguration configuration)
    {
        _logger = logger;
        _expressService = expressService;
        _redmineBotService = redmineBotService;
        _configuration = configuration;
    }

    public async Task ProcessIncomingMessageAsync(IncomingMessage message)
    {
        try
        {
            var messageText = message.Text ?? message.Body ?? "Без текста";
            var senderName = message.Sender?.Name ?? "Пользователь";
            
            _logger.LogInformation("Processing message from {Sender}: {Text}", senderName, messageText);

            var response = await _redmineBotService.ProcessMessageAsync(messageText, senderName);

            if (!string.IsNullOrEmpty(message.ChatId) && !string.IsNullOrEmpty(response))
            {
                await _expressService.SendMessageAsync(message.ChatId, response);
                _logger.LogInformation("Response sent to Express: {Response}", response);
            }
            else
            {
                _logger.LogWarning("No ChatId in message, response not sent");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message");
            
            if (!string.IsNullOrEmpty(message.ChatId))
            {
                await _expressService.SendMessageAsync(message.ChatId, $"Ошибка: {ex.Message}");
            }
        }
    }
}