namespace dbinfrastructure.models;

public class File
{
    public string File_id { get; set; }
    public string File_name { get; set; }
    public byte[] Content { get; set; }
}