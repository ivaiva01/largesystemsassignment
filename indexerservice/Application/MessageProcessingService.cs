using indexer.dto;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application;

public class MessageProcessingService : BackgroundService
{
    private readonly IMessageConsumer _consumer;
    private readonly IMessageProcessor _handler;
    private readonly ILogger<MessageProcessingService> _logger;

    public MessageProcessingService(IMessageConsumer consumer, IMessageProcessor handler, ILogger<MessageProcessingService> logger)
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
                var message = await _consumer.ConsumeAsync<EmailDto>(stoppingToken);
                await _handler.ProcessMessageAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message.");
            }
        }
    }
}
