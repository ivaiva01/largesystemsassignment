using cleaner.dto;
namespace cleaner.repositories;

public interface IMessagePublisher
{
    public Task PublishAsync<T>(MessageDto<T> message, CancellationToken cancellationToken);
}