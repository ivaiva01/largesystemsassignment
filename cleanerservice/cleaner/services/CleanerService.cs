namespace cleaner.services
{
    public class CleanerService
    {
        public string ExtractBody(string fileContent)
        {
            string[] lines = fileContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            bool isBody = false;
            string emailBody = "";

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    isBody = true;
                    continue;
                }
                if (isBody)
                {
                    emailBody += line + "\n";
                }
            }
            return emailBody.Trim();
        }
    }
}