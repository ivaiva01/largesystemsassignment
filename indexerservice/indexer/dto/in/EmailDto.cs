namespace indexer.dto;

public class EmailDto
{
    public string Id { get; set; }
    public string Body { get; set; }
    public string FileName { get; set; }
    public byte[] FileBytes { get; set; }
}