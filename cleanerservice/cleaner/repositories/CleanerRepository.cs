using System;
using System.Collections.Generic;
using System.IO;

namespace cleaner.repositories;

public class CleanerRepository
{
    private readonly string _directoryPath;

    public CleanerRepository(string directoryPath)
    {
        _directoryPath = directoryPath;
    }
    
    public List<string> GetTxtFiles()
    {
        if (!Directory.Exists(_directoryPath))
        {
            Console.WriteLine("Directory does not exist.");
            return new List<string>();
        }

        return new List<string>(Directory.GetFiles(_directoryPath, "*.txt"));
    }
    
    public void SaveExtractedEmails(string outputFilePath, List<string> emails)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(outputFilePath))
            {
                foreach (string email in emails)
                {
                    writer.WriteLine(email);
                }
            }
            Console.WriteLine($"Emails saved to: {outputFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving emails: {ex.Message}");
        }
    }
}