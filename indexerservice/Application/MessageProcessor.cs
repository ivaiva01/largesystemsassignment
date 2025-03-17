using Application;
using Domain;
using indexer.dto;
using Microsoft.Extensions.Logging;

namespace Infrastructure
{
    public class MessageProcessor : IMessageProcessor
    {
        private readonly IMessagePublisher _publisher;
        private readonly ITracingService _tracingService;
        private readonly ILogger<MessageProcessor> _logger;
        
        public MessageProcessor(IMessagePublisher publisher, ITracingService tracingService, ILogger<MessageProcessor> logger)
        {
            _publisher = publisher;
            _tracingService = tracingService;
            _logger = logger;
        }

        public async Task ProcessMessageAsync(MessageDto<EmailDto> message)
        {
            var activity = _tracingService.StartActivity("ProcessMessage");
            activity?.SetTag("message_id", message.Content.Id);
            _logger.LogInformation("Processing message {MessageId}", message.Content.Id);

            try
            {
                var email = new Email
                {
                    Id = message.Content.Id,
                    Body = message.Content.Body,
                    FileName = message.Content.FileName,
                    FileBytes = message.Content.FileBytes
                };
                
                var indexedFile = IndexEmail(email);
                
                var indexedFileDto = new MessageDto<IndexedFileDto>
                {
                    Content = new IndexedFileDto
                    {
                        Id = indexedFile.Id,
                        Filename = indexedFile.Filename,
                        Words = indexedFile.Words.Select(w => new WordDto
                        {
                            Text = w.Text,
                            Occurrences = w.Occurrences
                        }).ToList()
                    }
                };

                _logger.LogInformation("Message {MessageId} indexed successfully.", message.Content.Id);
                
                await _publisher.PublishAsync(indexedFileDto, default);

                _logger.LogInformation("Message {MessageId} forwarded successfully.", message.Content.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message {MessageId}", message.Content.Id);
                _tracingService.StopActivity(activity, hasError: true); // Mark trace as failed
            }
            finally
            {
                _tracingService.StopActivity(activity);
            }
        }

        private IndexedFile IndexEmail(Email email)
        {
            var indexingActivity = _tracingService.StartActivity("IndexEmail");
            indexingActivity?.SetTag("email_id", email.Id);

            _logger.LogInformation("Indexing email {EmailId}", email.Id);

            var wordOccurrences = new Dictionary<string, int>();
            var words = email.Body.Split(new char[] { ' ', '\t', '\n', '\r', '.', ',', ';', ':', '!', '?', '"', '(', ')', '[', ']', '{', '}' }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var word in words)
            {
                var cleanedWord = CleanWord(word);
                if (string.IsNullOrEmpty(cleanedWord))
                    continue;

                if (wordOccurrences.ContainsKey(cleanedWord))
                {
                    wordOccurrences[cleanedWord]++;
                }
                else
                {
                    wordOccurrences[cleanedWord] = 1;
                }
            }

            var indexedWords = wordOccurrences.Select(kvp => new Word
            {
                Text = kvp.Key,
                Occurrences = kvp.Value
            }).ToList();

            _logger.LogInformation("Indexing completed for email {EmailId}", email.Id);
            _tracingService.StopActivity(indexingActivity);

            return new IndexedFile
            {
                Id = email.Id,
                Filename = email.FileName,
                Words = indexedWords
            };
        }

        private string CleanWord(string word)
        {
            return new string(word.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray()).ToLower();
        }
    }
}