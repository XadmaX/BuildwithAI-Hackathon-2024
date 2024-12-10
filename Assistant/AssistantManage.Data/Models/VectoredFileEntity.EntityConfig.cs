using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace AssistantManage.Data.Models;

public class VectoredFileConfiguration : IEntityTypeConfiguration<VectoredFileEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<VectoredFileEntity> builder)
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
        
        builder.HasIndex(i => i.InModelId);
    }
}