using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace AssistantManage.Data.Models;

public class AssistantConfig : IEntityTypeConfiguration<AssistantEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AssistantEntity> builder)
    {
        builder.HasKey(k => k.Id);
        builder.Property(p => p.Id)
            .IsUnicode(false)
            .HasConversion<string>()
            .HasValueGenerator<SequentialGuidValueGenerator>()
            .ValueGeneratedOnAdd();

        builder.Property(p => p.CreatedAt)
            .ValueGeneratedOnAdd()
            .HasValueGenerator<CurrentDateTimeValueGenerator>();

        builder.Property(p => p.Status).HasConversion<string>();

        builder.HasIndex(i => i.InModelId);

        builder.HasOne(o => o.Store)
            .WithOne()
            .HasForeignKey<AssistantEntity>(f => f.StoreId);
    }
}