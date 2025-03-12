using System;
using System.Collections.Generic;
using System.IO;
using cleaner.models;
using cleaner.repositories;
using cleaner.services;

class Program
{
    static void Main()
    {
        string directoryPath = @"D:\Web Development\EASV\enron_mail_20150507\maildir\allen-p\_sent_mail";
        string outputFilePath = @"D:\Web Development\EASV\extracted_emails.txt";
        
        CleanerRepository cleanerRepository = new CleanerRepository(directoryPath);
        CleanerService cleanerService = new CleanerService();
        
        List<string> files = cleanerRepository.GetTxtFiles();
        List<string> allExtractedEmails = new List<string>();

        foreach (var file in files)
        {
            Console.WriteLine($"Processing file: {file}");
            List<CleanedEmail> emails = cleanerService.ExtractEmails(file);

            foreach (var email in emails)
            {
                Console.WriteLine($"Extracted: {email.EmailBody}");
                allExtractedEmails.Add(email.EmailBody);
                
            }
        }

        using (StreamWriter writer = new StreamWriter(outputFilePath))
        {
            foreach (string email in allExtractedEmails)
            {
                writer.WriteLine(email);
            }
        }
        
        Console.WriteLine("Extracted emails have been cleaned! Emails saved to: {outputFilePath}");
    }
}