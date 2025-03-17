using Application;
using Domain;
using indexer.dto;

namespace Infrastructure
{
    public class MessageProcessor : IMessageProcessor
    {
        private readonly IMessagePublisher _publisher;
        
        public MessageProcessor(IMessagePublisher publisher)
        {
            _publisher = publisher;
        }

        public async Task ProcessMessageAsync(MessageDto<EmailDto> message)
        {
            // Convert to domain object
            var email = new Email
            {
                Id = message.Content.Id,
                Body = message.Content.Body,
                FileName = message.Content.FileName,
                FileBytes = message.Content.FileBytes
            };
            
            // Index the email
            var indexedFile = IndexEmail(email);
            
            // Convert to DTO
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
            
            // Publish the indexed file for further processing
            await _publisher.PublishAsync(indexedFileDto, default);
        }

        private IndexedFile IndexEmail(Email email)
        {
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
