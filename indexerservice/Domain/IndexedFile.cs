namespace Domain;

public class IndexedFile
{
    public string Id { get; init; }
    public string Filename { get; init; }
    public List<Word> Words { get; init; }
}