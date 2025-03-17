namespace Domain;

public class IndexedFile
{
    public Guid Id { get; init; }
    public string Filename { get; init; }
    public List<Word> Words { get; init; }
}