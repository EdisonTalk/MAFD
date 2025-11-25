namespace PersistAgentThread.Infrastructure;

public sealed class CustomChatHistory
{
    public string Id { get; set; }
    public string Context { get; set; }
    public DateTime CreatedTime { get; set; } 

    public CustomChatHistory(string context)
    {
        Id = Guid.NewGuid().ToString();
        Context = context;
        CreatedTime = DateTime.UtcNow;
    }
}