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
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly string _queueName;
    private readonly ILogger<RabbitMqConsumer> _logger;

    public RabbitMqConsumer(RabbitMqSettings settings, ILogger<RabbitMqConsumer> logger)
    {
        _logger = logger;
        var factory = new ConnectionFactory
        {
            HostName = settings.Host,
            Port = settings.Port,
            UserName = settings.Username,
            Password = settings.Password
        };

        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
        _queueName = settings.QueueName;

        if (!string.IsNullOrWhiteSpace(settings.ExchangeName))
        {
            _channel.ExchangeDeclareAsync(settings.ExchangeName, ExchangeType.Direct, durable: true, autoDelete: false, arguments: null)
                .GetAwaiter().GetResult();
        }

        _channel.QueueDeclareAsync(_queueName, durable: false, exclusive: false, autoDelete: false, arguments: null)
            .GetAwaiter().GetResult();

        foreach (var key in settings.RoutingKeys)
        {
            _channel.QueueBindAsync(_queueName, settings.ExchangeName, key)
                .GetAwaiter().GetResult();
        }
    }

    public async Task<MessageDto<T>> ConsumeAsync<T>(CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<MessageDto<T>>();
        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var message = JsonSerializer.Deserialize<MessageDto<T>>(json);

                _logger.LogInformation("Received message: {Message}", json);

                await _channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken);
                tcs.TrySetResult(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message.");
                tcs.TrySetException(ex);
            }
        };

        await _channel.BasicConsumeAsync(_queueName, false, consumer, cancellationToken: cancellationToken);

        return await tcs.Task;
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
