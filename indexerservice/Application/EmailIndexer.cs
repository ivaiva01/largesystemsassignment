namespace Application;

public class EmailIndexer
{
    public void ProcessEmail(string jsonMessage)
    {
        Thread.Sleep(1000);
        Console.WriteLine("Forwarded indexed email");
    }
}