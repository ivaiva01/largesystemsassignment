using System.Diagnostics;
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
    private readonly ITracingService _tracingService;
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly string _exchangeName;
    private readonly RabbitMqSettings _settings;

    public RabbitMqPublisher(RabbitMqSettings settings, ILogger<RabbitMqPublisher> logger, Meter meter, ITracingService tracingService)
    {
        _logger = logger;
        _settings = settings;
        _exchangeName = settings.ExchangeName;
        _tracingService = tracingService;

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
        using var activity = _tracingService.StartActivity("PublishMessage");

        try
        {
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);
            var properties = new BasicProperties { Persistent = true };

            var exchange = string.IsNullOrWhiteSpace(_exchangeName) ? "" : _exchangeName;
            var routingKey = string.IsNullOrWhiteSpace(_exchangeName) ? _settings.QueueName : typeof(T).Name;
            
            string traceId = activity?.TraceId.ToString() ?? Guid.NewGuid().ToString();
            activity?.SetTag("trace_id", traceId);
            
            properties.Headers ??= new Dictionary<string, object?>();
            
            if (!properties.Headers.ContainsKey("trace_id"))
            {
                properties.Headers["trace_id"] = Encoding.UTF8.GetBytes(traceId);
            }

            _logger.LogInformation("Publishing message with Trace ID: {TraceId} to Exchange: {Exchange}, RoutingKey: {RoutingKey}, Message: {Message}",
                traceId, exchange, routingKey, json);

            await _channel.BasicPublishAsync(exchange, routingKey, true, properties, body, cancellationToken);

            _publishedMessagesCounter.Add(1);

            _logger.LogInformation("Message published successfully with Trace ID: {TraceId}.", traceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message.");
            activity?.SetTag("error", true);
            throw;
        }
        finally
        {
            _tracingService.StopActivity(activity);
        }
    }

    public void Dispose()
    {
        _logger.LogInformation("Disposing RabbitMQ publisher.");
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
