using indexer.dto;

namespace Application;

public interface IMessagePublisher
{
    public Task PublishAsync<T>(MessageDto<T> message, CancellationToken cancellationToken);
}