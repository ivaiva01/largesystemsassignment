namespace indexer.dto;

public class MessageDto<T>
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public T Content { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}