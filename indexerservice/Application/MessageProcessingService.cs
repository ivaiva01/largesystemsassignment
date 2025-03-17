using indexer.dto;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application;

public class MessageProcessingService : BackgroundService
{
    private readonly IMessageConsumer _consumer;
    private readonly IMessageProcessor _handler;
    private readonly ILogger<MessageProcessingService> _logger;
    private readonly ITracingService _tracingService;

    public MessageProcessingService(IMessageConsumer consumer, IMessageProcessor handler,
        ILogger<MessageProcessingService> logger, ITracingService tracingService)
    {
        _consumer = consumer;
        _handler = handler;
        _logger = logger;
        _tracingService = tracingService;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var activity = _tracingService.StartActivity("ConsumeAndProcessMessage");
            try
            {
                var message = await _consumer.ConsumeAsync<EmailDto>(stoppingToken);
                await _handler.ProcessMessageAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message.");
            }
            finally
            {
                _tracingService.StopActivity(activity);
            }
        }
    }
}
