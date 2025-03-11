using System.Text;
using apiCleaner.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace apiCleaner.Services;

public class RabbitMqService
{
    public string messageQueueName = "searchQueue";
    public async Task<List<Email>> GetEmailsWithSearch(string searchTerm)
    {
        //Contact the databaseservice using RabbitMQ
        var factory = new ConnectionFactory { HostName = "localhost", UserName = "guest", Password = "guest", Port = 5672 };
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();
        
        await channel.QueueDeclareAsync(queue: messageQueueName, durable: false, exclusive: false, autoDelete: false,
            arguments: null);

        string message = "Get all emails, with the search term: " + searchTerm;
        SearchTermDto testObj = new SearchTermDto
        {
            SearchTermText = "My test object"
        };
        // Create a composite object (message + TestObj) to send
        var combinedMessage = new
        {
            Message = message,
            TestObject = testObj
        };

        // Serialize combined message to JSON and then to bytes
        var jsonMessage = JsonConvert.SerializeObject(combinedMessage);
        var body = Encoding.UTF8.GetBytes(jsonMessage);
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: messageQueueName, body: body);
        
        return new List<Email>();
    }
}

public class SearchTermDto
{
    public string? SearchTermText { get; set; }
}