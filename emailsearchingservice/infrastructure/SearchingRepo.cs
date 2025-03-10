using System.Text;
using api.models;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace infrastructure;

public class SearchingRepo
{
    public string messageQueName = "searchQue";
    public async Task<List<Email>> GetEmailsWithSerarchterm(string searchTerm)
    {
        //Contact the databaseservice using RabbitMQ
        var factory = new ConnectionFactory { HostName = "localhost" };
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();
        
        await channel.QueueDeclareAsync(queue: messageQueName, durable: false, exclusive: false, autoDelete: false,
            arguments: null);

        string message = "Get all emails, with the search term: " + searchTerm;
        TestObj testObj = new TestObj
        {
            Text = "My test object"
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
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: messageQueName, body: body);
        
        return new List<Email>();
    }
}

public class TestObj
{
    public string? Text { get; set; }
}