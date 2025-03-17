using System.Text;
using System.Text.Json;
using Application;
using indexer.dto;
using Microsoft.Extensions.Configuration;
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

    public RabbitMqConsumer(IConfiguration configuration, ILogger<RabbitMqConsumer> logger)
    {
        _logger = logger;
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMq:Host"] ?? throw new ArgumentNullException("RabbitMq:Host"),
            Port = int.Parse(configuration["RabbitMq:Port"] ?? throw new ArgumentNullException("RabbitMq:Port")),
            UserName = configuration["RabbitMq:Username"] ?? throw new ArgumentNullException("RabbitMq:Username"),
            Password = configuration["RabbitMq:Password"] ?? throw new ArgumentNullException("RabbitMq:Password")
        };
        
        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
        _queueName = configuration["RabbitMq:QueueName"] ?? throw new ArgumentNullException("RabbitMq:QueueName");
        
        var exchangeName = configuration["RabbitMq:ExchangeName"] ?? throw new ArgumentNullException("RabbitMq:ExchangeName");

        _channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Direct, durable: true, autoDelete: false, arguments: null)
            .GetAwaiter().GetResult();
        _channel.QueueDeclareAsync(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null)
            .GetAwaiter().GetResult();
        
        _channel.QueueBindAsync(
            queue: _queueName,
            exchange: exchangeName,
            routingKey: "EmailDto" // TOODO: Fix hardcoding
        ).GetAwaiter().GetResult();
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

                await _channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken);
                tcs.SetResult(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message.");
                tcs.SetException(ex);
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
