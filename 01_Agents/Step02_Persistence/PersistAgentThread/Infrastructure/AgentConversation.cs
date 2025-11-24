namespace PersistAgentThread.Infrastructure;

public sealed class AgentConversation
{
    public string Id { get; set; }
    public string Context { get; set; }
    public DateTime CreatedTime { get; set; } 

    public AgentConversation(string context)
    {
        Id = Guid.NewGuid().ToString();
        Context = context;
        CreatedTime = DateTime.UtcNow;
    }
}