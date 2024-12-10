using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace AssistantManage.Data.Models;

public class VectorStoreEntityConfig : IEntityTypeConfiguration<VectorStoreEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<VectorStoreEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(p => p.Id)
            .IsUnicode(false)
            .HasConversion<string>()
            .HasValueGenerator<SequentialGuidValueGenerator>()
            .ValueGeneratedOnAdd();

        builder.Property(p => p.CreatedAt)
            .ValueGeneratedOnAdd()
            .HasValueGenerator<CurrentDateTimeValueGenerator>();

        builder.HasIndex(i => i.InModelId);

        builder.HasMany(m => m.Files).WithMany();
    }
}