using Microsoft.EntityFrameworkCore;

namespace PersistAgentThread.Infrastructure;

public class AgentConversationDbContext : DbContext
{
    public DbSet<AgentConversation> AgentConversations { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=AgentConversationDb.db");
    }
}