using System.Reflection;
using AssistantManage.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace AssistantManage.Data;

public class AssistantDbContext(DbContextOptions<AssistantDbContext> options) : DbContext(options)
{
    public DbSet<VectoredFileEntity> Files { get; set; }
    public DbSet<AssistantEntity> Assistants { get; set; }
    public DbSet<MessageEntity> Messages { get; set; }
    public DbSet<ConversationEntity> Conversations { get; set; }
    public DbSet<VectorStoreEntity> VectorStores { get; set; }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}