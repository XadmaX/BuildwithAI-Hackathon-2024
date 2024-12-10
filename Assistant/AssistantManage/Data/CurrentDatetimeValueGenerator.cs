using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace AssistantManage.Data;

public class CurrentDateTimeValueGenerator : ValueGenerator<DateTime>
{
    public override DateTime Next(EntityEntry entry) => DateTime.UtcNow;
    public override bool GeneratesTemporaryValues => false;
}

public class YourEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class YourEntityConfiguration : IEntityTypeConfiguration<YourEntity>
{
    public void Configure(EntityTypeBuilder<YourEntity> builder)
    {
        builder.Property(e => e.CreatedAt)
            .HasValueGenerator<CurrentDateTimeValueGenerator>();
    }
}