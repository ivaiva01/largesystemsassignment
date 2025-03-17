using indexer.dto;

namespace Application;

public interface IMessageConsumer
{
    public Task<MessageDto<T>> ConsumeAsync<T>(CancellationToken cancellationToken);
}