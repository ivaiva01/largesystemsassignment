using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using cleaner.repositories;
using cleaner.dto;


namespace cleaner.services;

public class RabbitMqPublisher : IMessagePublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly string _exchangeName;
    private readonly RabbitMqSettings _settings;

    public RabbitMqPublisher(RabbitMqSettings settings)
    {
        _settings = settings;
        _exchangeName = settings.ExchangeName;

        var factory = new ConnectionFactory
        {
            HostName = settings.Host,
            Port = settings.Port,
            UserName = settings.Username,
            Password = settings.Password
        };

        try
        {
            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

            if (!string.IsNullOrWhiteSpace(_exchangeName))
            {
                _channel.ExchangeDeclareAsync(_exchangeName, ExchangeType.Direct, durable: true, autoDelete: false, arguments: null)
                    .GetAwaiter().GetResult();
            }
        }
        catch (Exception ex)
        {
            
            throw;
        }

        _channel.BasicReturnAsync += async (sender, ea) =>
        {
            var returnedMessage = ea.Body.Length > 0 ? Encoding.UTF8.GetString(ea.Body.ToArray()) : "(empty message)";
            await Task.CompletedTask;
        };
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
            
            await _channel.BasicPublishAsync(exchange, routingKey, true, properties, body, cancellationToken);
        }
        catch (Exception ex)
        {
           
            throw;
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}