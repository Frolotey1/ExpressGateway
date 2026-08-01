namespace ExpressGateway.Services;

public class MessagePollingService : BackgroundService
{
    private readonly ILogger<MessagePollingService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly TimeSpan _pollingInterval;
    private readonly string _chatId;
    private DateTime _lastCheckTime = DateTime.UtcNow.AddMinutes(-5);
    private readonly int _maxMessagesPerPoll = 50;
    private bool _isProcessing = false;

    public MessagePollingService(
        ILogger<MessagePollingService> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        
        _chatId = configuration["ExpressSettings:ChatId"] ?? "";
        var interval = configuration.GetValue<int>("PollingSettings:IntervalSeconds", 5);
        _pollingInterval = TimeSpan.FromSeconds(interval);
        
        _maxMessagesPerPoll = configuration.GetValue<int>("PollingSettings:MaxMessagesPerPoll", 50);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Message Polling Service started");
        _logger.LogInformation("Chat ID: {ChatId}", _chatId);
        _logger.LogInformation("Interval: {Interval} seconds", _pollingInterval.TotalSeconds);
        _logger.LogInformation("Max messages per poll: {MaxMessages}", _maxMessagesPerPoll);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PollMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in polling loop");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }
        
        _logger.LogInformation("Message Polling Service stopped");
    }

    private async Task PollMessagesAsync(CancellationToken stoppingToken)
    {
        if (_isProcessing)
        {
            _logger.LogWarning("Previous polling still in progress, skipping...");
            return;
        }

        _isProcessing = true;
        
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var expressService = scope.ServiceProvider.GetRequiredService<IExpressService>();
            var processor = scope.ServiceProvider.GetRequiredService<MessageProcessorService>();

            if (string.IsNullOrEmpty(_chatId))
            {
                _logger.LogWarning("ChatId not configured, skipping poll");
                return;
            }

            var messages = await expressService.GetNewMessagesAsync(_chatId, _lastCheckTime);
            
            if (messages.Any())
            {
                _logger.LogInformation("Received {Count} new messages", messages.Count);
                
                _lastCheckTime = DateTime.UtcNow;

                var messagesToProcess = messages.Take(_maxMessagesPerPoll).ToList();
                
                if (messages.Count > _maxMessagesPerPoll)
                {
                    _logger.LogWarning("Too many messages ({Count}), processing first {Max}", 
                        messages.Count, _maxMessagesPerPoll);
                }

                foreach (var message in messagesToProcess)
                {
                    if (stoppingToken.IsCancellationRequested)
                        break;
                        
                    try
                    {
                        await processor.ProcessIncomingMessageAsync(message);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Error processing message from {Sender}", 
                            message.Sender?.Name ?? "Unknown");
                    }
                }
            }
            else
            {
                _lastCheckTime = DateTime.UtcNow;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error polling messages");
        }
        finally
        {
            _isProcessing = false;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Message Polling Service...");
        
        while (_isProcessing)
        {
            await Task.Delay(100, cancellationToken);
        }
        
        await base.StopAsync(cancellationToken);
    }
}