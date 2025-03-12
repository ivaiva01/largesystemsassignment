using System.IO;
using System;
using System.Text.RegularExpressions;
using cleaner.models;

namespace cleaner.services;

public class CleanerService
{
    public List<CleanedEmail> ExtractEmails(string filePath)
    {
        List<CleanedEmail> extractedEmails = new List<CleanedEmail>();

        if (!File.Exists(filePath))
        {
            Console.WriteLine($"File not found: {filePath}");
            return extractedEmails;
        }

        using (StreamReader sr = new StreamReader(filePath))
        {
            string line;
            bool isBody = true;
            string emailBody = "";
            string header = "";

            while ((line = sr.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    isBody = true;
                    continue;
                }

                if (isBody)
                    emailBody += line + "\n";
                else
                    header += line + "\n";
            }

            RawEmailFile emailFile = new RawEmailFile
            {
                Header = header.Trim(),
                Body = emailBody.Trim()
            };
            
            string emailPattern = @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}";
            MatchCollection matches = Regex.Matches(emailFile.Body, emailPattern);

            foreach (Match match in matches)
            {
                extractedEmails.Add(new CleanedEmail { EmailBody = match.Value });
            }
                
        }
        return extractedEmails;
    }
}