using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace dbservice;

public class rabbitMQIncomming
{

    public async Task rabbitMQIncommingAwaiter()
    {
        
        var factory = new ConnectionFactory { HostName = "localhost" };
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        string messageQueName = "searchQue";

        await channel.QueueDeclareAsync(
            queue: messageQueName, 
            //durable: true,                    //This means, that even if the server stops, the que or messages will not be lost
            exclusive: false,
            autoDelete: false,
            arguments: null);
    

        Console.WriteLine(" [*] Waiting for messages.");

        var consumer = new AsyncEventingBasicConsumer(channel);


        consumer.ReceivedAsync += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($" [x] Received {message}");
    
            channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
            return Task.CompletedTask;
        };

        await channel.BasicConsumeAsync(
            messageQueName, 
            autoAck: false,                 //Here we tell RabbitMQ, they are not allowed to delete the message right now 
            consumer: consumer
        );
    
        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
    }
}