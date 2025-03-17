using dbinfrastructure.models;
using File = dbinfrastructure.models.File;


namespace dbinfrastructure;

public interface IDatabaseRepo
{
    IEnumerable<File> GetFilesContainingSearchTerm(string searchTerm);
    
    File AddFile(File file);
    
    void AddWord(string newWord);
    
    bool DoesWordExist(string word);
    
    File? GetFileWithId(string id);
    
    IEnumerable<Word> GetAllWords();
}