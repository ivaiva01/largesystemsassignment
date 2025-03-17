using System;
using System.Text;
using RabbitMQ.Client;

namespace cleaner.services
{
    public class CleanerService : IDisposable
    {
        private readonly IChannel _channel;
        private readonly IConnection _connection;
        private const string QueueName = "email_bodies_queue";

        private CleanerService(IConnection connection, IChannel channel)
        {
            _connection = connection;
            _channel = channel;
        }

        public static async Task<CleanerService> CreateAsync()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: QueueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            return new CleanerService(connection, channel);
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

        public async Task SendToQueue(string emailBody)
        {
            var body = Encoding.UTF8.GetBytes(emailBody);
            try
            {
                await _channel.BasicPublishAsync(exchange: string.Empty,
                    routingKey: QueueName,
                    body: body,
                    basicProperties: null,
                    mandatory: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine($"[x] Sent: {emailBody}");
        }

        public void Dispose()
        {
            _channel?.CloseAsync();
            _connection?.CloseAsync();
        }
    }
}