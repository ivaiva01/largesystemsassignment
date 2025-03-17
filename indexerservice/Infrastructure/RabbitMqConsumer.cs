using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text;
using System.Text.Json;
using Application;
using indexer.dto;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Infrastructure;

public class RabbitMqConsumer : IMessageConsumer, IDisposable
{
    private readonly ILogger<RabbitMqConsumer> _logger;
    private readonly Counter<int> _consumedMessagesCounter;
    private readonly Histogram<double> _messageProcessingTime;
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly string _queueName;

    public RabbitMqConsumer(RabbitMqSettings settings, ILogger<RabbitMqConsumer> logger, Meter meter)
    {
        _logger = logger;
        _queueName = settings.QueueName;
        
        _consumedMessagesCounter = meter.CreateCounter<int>("consumed_messages", "messages", "Total messages consumed");`

        var factory = new ConnectionFactory
        {
            HostName = settings.Host,
            Port = settings.Port,
            UserName = settings.Username,
            Password = settings.Password
        };

        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
        _logger.LogInformation("RabbitMQ consumer connected.");

        if (!string.IsNullOrWhiteSpace(settings.ExchangeName))
        {
            _channel.ExchangeDeclareAsync(settings.ExchangeName, ExchangeType.Direct, durable: true, autoDelete: false, arguments: null)
                .GetAwaiter().GetResult();
            _logger.LogInformation("Exchange {ExchangeName} declared.", settings.ExchangeName);
        }

        _channel.QueueDeclareAsync(_queueName, durable: false, exclusive: false, autoDelete: false, arguments: null)
            .GetAwaiter().GetResult();
        _logger.LogInformation("Queue {QueueName} declared.", _queueName);
    }

    public async Task<MessageDto<T>> ConsumeAsync<T>(CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<MessageDto<T>>();
        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            var stopwatch = Stopwatch.StartNew();

            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);

            _logger.LogInformation("Received message from queue {QueueName}: {Message}", _queueName, json);

            try
            {
                var message = JsonSerializer.Deserialize<MessageDto<T>>(json);
                
                _consumedMessagesCounter.Add(1);

                await _channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken);

                _logger.LogInformation("Message successfully processed and acknowledged.");
                tcs.TrySetResult(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message: {Message}", json);
                tcs.TrySetException(ex);
            }
        };

        await _channel.BasicConsumeAsync(_queueName, false, consumer, cancellationToken: cancellationToken);
        return await tcs.Task;
    }

    public void Dispose()
    {
        _logger.LogInformation("Disposing RabbitMQ consumer.");
        _channel?.Dispose();
        _connection?.Dispose();
    }
}


