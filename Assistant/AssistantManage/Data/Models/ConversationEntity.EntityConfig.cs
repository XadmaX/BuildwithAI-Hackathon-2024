using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Newtonsoft.Json;

namespace AssistantManage.Data.Models;

public class ConversationConfig : IEntityTypeConfiguration<ConversationEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ConversationEntity> builder)
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

        builder.Property(p => p.Documents)
            .HasConversion(
                @in => JsonConvert.SerializeObject(@in),
                @out => JsonConvert.DeserializeObject<List<string>>(@out));

        builder.HasIndex(i => i.ThreadId);

        builder.HasMany(m => m.Messages)
            .WithOne()
            .HasForeignKey(k => k.ConversationId);
        builder.HasOne(o => o.Assistant)
            .WithMany()
            .HasForeignKey(k => k.AssistantId);
    }
}