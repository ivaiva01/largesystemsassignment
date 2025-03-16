using Application;
using indexer.dto;

namespace Infrastructure;

public class MessageProcessor : IMessageProcessor
{
    private readonly IMessagePublisher _publisher;
    
    public MessageProcessor(IMessagePublisher publisher)
    {
        _publisher = publisher;
    }

    public Task ProcessMessageAsync(MessageDto<EmailDto> message)
    {
        Console.WriteLine("Processing a message");
        return Task.CompletedTask;
    }
}