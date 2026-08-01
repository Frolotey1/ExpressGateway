// Services/MessageProcessorService.cs
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
    private readonly HashSet<string> _processedMessageIds = new();
    private readonly object _lockObject = new();

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
            var messageId = message.Id ?? $"{message.ChatId}_{DateTime.UtcNow.Ticks}";
            
            lock (_lockObject)
            {
                if (_processedMessageIds.Contains(messageId))
                {
                    _logger.LogWarning("⏭Duplicate message detected: {MessageId}", messageId);
                    return;
                }
                
                _processedMessageIds.Add(messageId);
                
                if (_processedMessageIds.Count > 1000)
                {
                    var toRemove = _processedMessageIds.Take(100).ToList();
                    foreach (var id in toRemove)
                    {
                        _processedMessageIds.Remove(id);
                    }
                }
            }

            var chatId = message.ChatId ?? _configuration["ExpressSettings:ChatId"] ?? "";
            var messageText = message.Text ?? message.Body ?? "Без текста";
            var senderName = message.Sender?.Name ?? "Пользователь";
            
            var isCommand = messageText.StartsWith("/");
            
            _logger.LogInformation("Processing message from {Sender}: {Text} (Command: {IsCommand})", 
                senderName, messageText, isCommand);

            if (!isCommand)
            {
                await SendTypingIndicatorAsync(chatId);
            }

            var response = await _redmineBotService.ProcessMessageAsync(messageText, senderName, chatId);

            if (!string.IsNullOrEmpty(response))
            {
                await SendResponseAsync(chatId, response);
                _logger.LogInformation("Response sent to Express: {Response}", response);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message from {Sender}", 
                message.Sender?.Name ?? "Unknown");

            try
            {
                var chatId = message.ChatId ?? _configuration["ExpressSettings:ChatId"] ?? "";
                await SendErrorResponseAsync(chatId, ex.Message);
            }
            catch (Exception sendEx)
            {
                _logger.LogError(sendEx, "Failed to send error response");
            }
        }
    }

    private async Task SendTypingIndicatorAsync(string chatId)
    {
        try
        {
            await _expressService.SendMessageAsync(chatId, "Обрабатываю ваш запрос");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send typing indicator");
        }
    }

    private async Task SendResponseAsync(string chatId, string response)
    {
        try
        {
            const int maxMessageLength = 4000;
            
            if (response.Length <= maxMessageLength)
            {
                await _expressService.SendMessageAsync(chatId, response);
                return;
            }

            var parts = SplitMessage(response, maxMessageLength);
            foreach (var part in parts)
            {
                await _expressService.SendMessageAsync(chatId, part);
                await Task.Delay(100); 
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send response to Express");
            throw;
        }
    }

    private async Task SendErrorResponseAsync(string chatId, string errorMessage)
    {
        var response = $"Произошла ошибка:\n```\n{errorMessage}\n```\n\n" +
                       "Пожалуйста, попробуйте позже или обратитесь к администратору.";
        
        await SendResponseAsync(chatId, response);
    }

    private List<string> SplitMessage(string message, int maxLength)
    {
        var parts = new List<string>();
        
        for (int i = 0; i < message.Length; i += maxLength)
        {
            var part = message.Substring(i, Math.Min(maxLength, message.Length - i));
            parts.Add(part);
        }
        
        return parts;
    }

    public async Task ProcessAttachmentAsync(string chatId, string fileName, byte[] fileContent)
    {
        try
        {
            _logger.LogInformation("📎 Processing attachment: {FileName} ({Size} bytes)", 
                fileName, fileContent.Length);

            var response = $"Получен файл: {fileName}\n" +
                          $"Размер: {fileContent.Length / 1024} KB\n" +
                          "Файл будет прикреплен к заявке.";

            await SendResponseAsync(chatId, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to process attachment");
            throw;
        }
    }
}