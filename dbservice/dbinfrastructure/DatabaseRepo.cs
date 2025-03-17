using System.Data;
using System.Data.SqlTypes;
using Dapper;
using dbinfrastructure.models;
using Microsoft.Data.SqlClient;
using File = dbinfrastructure.models.File;

namespace dbinfrastructure;

public class DatabaseRepo : IDatabaseRepo
{
    private readonly string? _connectionString;

    public DatabaseRepo(string? connectionString)
    {
        _connectionString = connectionString;
    }
    public IEnumerable<File> GetFilesContainingSearchTerm(string searchTerm)
    {
        string sql = @"
        SELECT DISTINCT * 
        FROM Files
        INNER JOIN Occurrences ON Files.file_id = Occurrences.file_id
        INNER JOIN Words ON Occurrences.word_id = Words.word_id
        WHERE LOWER(Words.word) LIKE LOWER(@searchTerm)";

        try
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                return connection.Query<File>(sql, new { searchTerm = "%" + searchTerm + "%" });
            }
        }
        catch (Exception ex)
        {
            throw new DataException("Failed to fetch files containing the search term", ex);
        }
    }

    public File AddFile(File file)
    {
        try
        {
            string sql = $@"
                    INSERT INTO Files (file_id, file_name, content) 
                    VALUES (@file_id, @file_name, @content)
                     
                    ";
            using (var connection = new SqlConnection(_connectionString))
            {
                return connection.QuerySingle<File>(sql,
                    new { @file_id = file.File_id, @file_name = file.File_name, @content = file.Content });
            }
        }
        catch (Exception e)
        {
            throw new SqlTypeException("Failed to register the new file", e);
        }
    }

    public void AddWord(string newWord)
    {
        try
        {
            string sql = $@"INSERT INTO Words 
                        values (@word)
                        ;";
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Execute(sql, new { @word = newWord });
            }
        }
        catch (Exception e)
        {
            throw new SqlTypeException("Failed to register new word", e);
        }
    }


    public bool DoesWordExist(string word)
    {
        try
        {
            var sql = "SELECT COUNT(*) FROM Words WHERE LOWER(word) = LOWER(@word)";
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                int count = connection.ExecuteScalar<int>(sql, new { @word = word });
                // returns true if group exists
                return count > 0;
            }
        }
        catch (Exception e)
        {
            throw new SqlTypeException("Failed to see if the word was already in the database", e);
        }
    }


    public File? GetFileWithId(string id)
    {
        try
        {
            string sql = "SELECT * FROM Files WHERE file_id = @id";

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open(); // Opens the connection
                return conn.QuerySingleOrDefault<File>(sql, new { Id = id });
            }
        }
        catch (Exception e)
        {
            throw new DataException($"Failed to return the file with the Id: {id}", e);
        }
    }

    public IEnumerable<Word> GetAllWords()
    {
        try
        {
            string sql = "SELECT * FROM Words";

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open(); // Opens the connection
                return conn.Query<Word>(sql);
            }
        }
        catch (Exception e)
        {
            throw new DataException("Failed to fetch words", e);
        }
    }
}