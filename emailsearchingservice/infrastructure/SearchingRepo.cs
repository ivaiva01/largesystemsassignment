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
        List<Email> emails = GetTextData();
        
        // Create an object (message + TestObj) to send to database service
        var combinedMessage = new
        {
            Message = message,
            TestObject = emails[0].EmailBody
        };

        // Serialize combined message to JSON and then to bytes
        var jsonMessage = JsonConvert.SerializeObject(combinedMessage);
        var body = Encoding.UTF8.GetBytes(jsonMessage);
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: messageQueName, body: body);
        
        return emails;
    }

    private List<Email> GetTextData()
    {
        List<Email> emails = new List<Email>();
        Email email1 = new Email
        {
            FileId = 1,
            EmailBody = "This is the email body of the first test email",
            
        };
        Email email2 = new Email
        {
            FileId = 2,
            EmailBody = "This is the email body of the second test email",
        };
        Email email3 = new Email
        {
            FileId = 3,
            EmailBody = "This is the email body of the third test email",
        };

        emails.Add(email1);
        emails.Add(email2);
        emails.Add(email3);
        return emails;
    }
}

public class SearchTermDto
{
    public string? SearchTermText { get; set; }
}

