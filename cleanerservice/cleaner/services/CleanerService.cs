using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace cleaner.services
{
    public class CleanerService : IDisposable
    {
        private readonly IChannel _channel;
        private readonly IConnection _connection;
        private const string QueueName = "email_bodies_queue";
        private readonly string _exchangeName;

        private CleanerService(IConnection connection, IChannel channel, string exchangeName)
        {
            _connection = connection;
            _channel = channel;
            _exchangeName = exchangeName;
        }
        
        public static async Task<CleanerService> CreateAsync(string exchangeName = "")
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            IConnection connection = null;
            IChannel channel = null;

            try
            {
                connection = await factory.CreateConnectionAsync();
                channel = await connection.CreateChannelAsync();

                if (!string.IsNullOrWhiteSpace(exchangeName))
                {
                    await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Direct, durable: true, autoDelete: false, arguments: null);
                }
                
                await channel.QueueDeclareAsync(queue: QueueName,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                return new CleanerService(connection, channel, exchangeName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating connection or channel: {ex.Message}");
                throw; 
            }
        }
        
        public string ExtractBody(string fileContent)
        {
            string[] lines = fileContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            bool isBody = false;
            string emailBody = "";

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    isBody = true;
                    continue;
                }
                if (isBody)
                {
                    emailBody += line + "\n";
                }
            }
            return emailBody.Trim();
        }
        
        public async Task SendToQueueAsync(string emailBody, CancellationToken cancellationToken)
        {
            var body = Encoding.UTF8.GetBytes(emailBody);
            try
            {
                string exchange = string.IsNullOrWhiteSpace(_exchangeName) ? "" : _exchangeName;
                string routingKey = QueueName;

                await _channel.BasicPublishAsync(
                    exchange: exchange,
                    routingKey: routingKey,
                    mandatory: true,
                    body: body,
                    cancellationToken: cancellationToken);

                Console.WriteLine($"[x] Sent: {emailBody}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message to queue: {ex.Message}");
            }
        }
        
        public async Task DisposeAsync()
        {
            if (_channel != null)
            {
                await _channel.CloseAsync();
            }
            if (_connection != null)
            {
                await _connection.CloseAsync();
            }
        }
        
        public void Dispose()
        {
            DisposeAsync().GetAwaiter().GetResult();
        }
    }
}
