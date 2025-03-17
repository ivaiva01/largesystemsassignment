namespace indexer.dto;

public class IndexedFileDto
{
    public string Id { get; init; }
    public string Filename { get; init; }
    public List<WordDto> Words { get; init; }
}