using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Newtonsoft.Json;

namespace AssistantManage.Data.Models;

public class MessageConfig : IEntityTypeConfiguration<MessageEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<MessageEntity> builder)
    {
        builder.HasKey(k => k.Id);
        builder.Property(p => p.Id)
            .IsUnicode(false)
            .HasConversion<string>()
            .HasValueGenerator<SequentialGuidValueGenerator>()
            .ValueGeneratedOnAdd();

        builder.Property(p => p.Sender)
            .HasConversion<string>();

        builder.Property(p => p.CreatedAt)
            .ValueGeneratedOnAdd()
            .HasValueGenerator<CurrentDateTimeValueGenerator>();

        builder.Property(p => p.Annotations)
            .HasConversion(
                c => JsonConvert.SerializeObject(c),
                c => JsonConvert.DeserializeObject<List<Annotation>>(c)!);

        builder.HasIndex(i => i.AssistantMessageId);

        builder.HasOne<ConversationEntity>()
            .WithMany(m => m.Messages)
            .HasForeignKey(f => f.ConversationId);
    }
}