using indexer.dto;

namespace Application;

public interface IMessageProcessor
{
    Task ProcessMessageAsync(MessageDto<EmailDto> message);
}