using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using cleaner.repositories;
using cleaner.services;

class Program
{
    static async Task Main()
    {
        string directoryPath = @"D:\\Web Development\\EASV\\enron_mail_20150507\\maildir\\allen-p\\_sent_mail";
        string outputFilePath = @"D:\\Web Development\\EASV\\extracted_email_bodies.txt";
        
        CleanerRepository cleanerRepository = new CleanerRepository(directoryPath);
        
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = cancellationTokenSource.Token;
        
        using (CleanerService cleanerService = await CleanerService.CreateAsync())
        {
            List<string> files = cleanerRepository.GetFiles();
            List<string> allExtractedBodies = new List<string>();

            
            foreach (var file in files)
            {
                Console.WriteLine($"Processing file: {file}");

                string fileContent = File.ReadAllText(file);
                string body = cleanerService.ExtractBody(fileContent);
                
                if (!string.IsNullOrWhiteSpace(body))
                {
                    Console.WriteLine($"Extracted body: {body}");
                    allExtractedBodies.Add(body);
                    
                    await cleanerService.SendToQueueAsync(body, cancellationToken);
                }
            }
            
            cleanerRepository.SaveExtractedEmails(outputFilePath, allExtractedBodies);

            Console.WriteLine($"Extraction complete! Bodies saved to: {outputFilePath} and sent to RabbitMQ.");
        }
    }
}
