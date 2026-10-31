using AiCallCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiCallCenter.Infrastructure.Configurations;

public sealed class AiConversationMessageConfiguration
    : IEntityTypeConfiguration<AiConversationMessage>
{
    public void Configure(
        EntityTypeBuilder<AiConversationMessage> builder)
    {
        builder.ToTable("AiConversationMessages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.ConversationId)
            .IsRequired();

        builder.Property(x => x.Speaker)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Content)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.SequenceNumber)
            .IsRequired();

        builder.Property(x => x.SentAt)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .IsRequired();

        builder.HasIndex(x => x.ConversationId);

        builder.HasIndex(x => new
        {
            x.ConversationId,
            x.SequenceNumber
        })
        .IsUnique();

        builder.HasOne(x => x.Conversation)
            .WithMany(x => x.Messages)
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}