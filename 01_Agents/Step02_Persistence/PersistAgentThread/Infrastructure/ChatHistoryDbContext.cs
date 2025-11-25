using Microsoft.EntityFrameworkCore;

namespace PersistAgentThread.Infrastructure;

public class ChatHistoryDbContext : DbContext
{
    public DbSet<CustomChatHistory> ChatHistories { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=ChatHistoryDb.db");
    }
}