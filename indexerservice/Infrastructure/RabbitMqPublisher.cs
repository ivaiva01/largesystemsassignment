using System.Text;
using System.Text.Json;
using Application;
using indexer.dto;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

public class RabbitMqPublisher : IMessagePublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly string _exchangeName;
    private readonly ILogger<RabbitMqPublisher> _logger;

    public RabbitMqPublisher(IConfiguration configuration, ILogger<RabbitMqPublisher> logger)
    {
        _logger = logger;
        
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMq:Host"] ?? throw new ArgumentNullException("RabbitMq:Host"),
            Port = int.Parse(configuration["RabbitMq:Port"] ?? throw new ArgumentNullException("RabbitMq:Port")),
            UserName = configuration["RabbitMq:Username"] ?? throw new ArgumentNullException("RabbitMq:Username"),
            Password = configuration["RabbitMq:Password"] ?? throw new ArgumentNullException("RabbitMq:Password")
        };
        
        _exchangeName = configuration["RabbitMq:ExchangeName"] ?? throw new ArgumentNullException("RabbitMq:ExchangeName");
        
        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
        
        _channel.ExchangeDeclareAsync(_exchangeName, ExchangeType.Direct, durable: true, autoDelete: false, arguments: null)
                    .GetAwaiter().GetResult();
        
        _channel.BasicReturnAsync += async (sender, ea) =>
        {
            var returnedMessage = ea.Body.Length > 0 ? Encoding.UTF8.GetString(ea.Body.ToArray()) : "(empty message)";
            _logger.LogWarning("Message returned: {Message}, Reason: {ReplyText}", returnedMessage, ea.ReplyText);
            await Task.CompletedTask;
        };

    }
    
    
    public async Task PublishAsync<T>(MessageDto<T> message, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = new BasicProperties { Persistent = true };
        
        await _channel.BasicPublishAsync(_exchangeName,
            routingKey: typeof(T).Name, true, properties, body, cancellationToken);
    }
    
    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}