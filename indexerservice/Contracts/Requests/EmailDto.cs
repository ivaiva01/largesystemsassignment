namespace indexer.dto;

public class EmailDto
{
    public string Id { get; init; }
    public string Body { get; init; }
    public string FileName { get; init; }
    public byte[] FileBytes { get; init; }
}