using indexer.dto;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application;

public class RabbitMqBackgroundService : BackgroundService
{
    private readonly IMessageConsumer _consumer;
    private readonly IMessageProcessor _handler;
    private readonly ILogger<RabbitMqBackgroundService> _logger;

    public RabbitMqBackgroundService(IMessageConsumer consumer, IMessageProcessor handler, ILogger<RabbitMqBackgroundService> logger)
    {
        _consumer = consumer;
        _handler = handler;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var message = await _consumer.ConsumeAsync<MessageDto<EmailDto>>(stoppingToken);
                await _handler.ProcessMessageAsync(message.Content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message.");
            }
        }
    }
}
