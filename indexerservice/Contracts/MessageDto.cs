namespace indexer.dto;

public class MessageDto<T>
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public T Content { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}