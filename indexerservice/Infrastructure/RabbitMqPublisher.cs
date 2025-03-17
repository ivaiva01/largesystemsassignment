using System.Diagnostics.Metrics;
using System.Text;
using System.Text.Json;
using Application;
using indexer.dto;
using Infrastructure;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

public class RabbitMqPublisher : IMessagePublisher, IDisposable
{
    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly Counter<int> _publishedMessagesCounter;
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly string _exchangeName;
    private readonly RabbitMqSettings _settings;

    public RabbitMqPublisher(RabbitMqSettings settings, ILogger<RabbitMqPublisher> logger, Meter meter)
    {
        _logger = logger;
        _settings = settings;
        _exchangeName = settings.ExchangeName;
        
        _publishedMessagesCounter = meter.CreateCounter<int>("published_messages", "messages", "Total messages published");

        var factory = new ConnectionFactory
        {
            HostName = settings.Host,
            Port = settings.Port,
            UserName = settings.Username,
            Password = settings.Password
        };

        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
        _logger.LogInformation("RabbitMQ publisher connected.");

        if (!string.IsNullOrWhiteSpace(_exchangeName))
        {
            _channel.ExchangeDeclareAsync(_exchangeName, ExchangeType.Direct, durable: true, autoDelete: false, arguments: null)
                .GetAwaiter().GetResult();
            _logger.LogInformation("Exchange {ExchangeName} declared successfully.", _exchangeName);
        }
    }

    public async Task PublishAsync<T>(MessageDto<T> message, CancellationToken cancellationToken)
    {
        try
        {
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);
            var properties = new BasicProperties { Persistent = true };

            var exchange = string.IsNullOrWhiteSpace(_exchangeName) ? "" : _exchangeName;
            var routingKey = string.IsNullOrWhiteSpace(_exchangeName) ? _settings.QueueName : typeof(T).Name;

            _logger.LogInformation("Publishing message to exchange: {Exchange}, RoutingKey: {RoutingKey}, Message: {Message}",
                exchange, routingKey, json);

            await _channel.BasicPublishAsync(exchange, routingKey, true, properties, body, cancellationToken);
            
            _publishedMessagesCounter.Add(1);

            _logger.LogInformation("Message published successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message.");
            throw;
        }
    }

    public void Dispose()
    {
        _logger.LogInformation("Disposing RabbitMQ publisher.");
        _channel?.Dispose();
        _connection?.Dispose();
    }
}

