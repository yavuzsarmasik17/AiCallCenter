using AiCallCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiCallCenter.Infrastructure.Configurations;

public sealed class AiConversationConfiguration
    : IEntityTypeConfiguration<AiConversation>
{
    public void Configure(
        EntityTypeBuilder<AiConversation> builder)
    {
        builder.ToTable("AiConversations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.CompanyId)
            .IsRequired();

        builder.Property(x => x.AiAgentId)
            .IsRequired();

        builder.Property(x => x.CustomerName)
            .HasMaxLength(200);

        builder.Property(x => x.CustomerPhone)
            .HasMaxLength(30);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.StartedAt)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .IsRequired();

        builder.HasIndex(x => x.CompanyId);

        builder.HasIndex(x => x.AiAgentId);

        builder.HasIndex(x => x.StartedAt);

        builder.HasOne(x => x.Company)
            .WithMany(x => x.Conversations)
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AiAgent)
            .WithMany(x => x.Conversations)
            .HasForeignKey(x => x.AiAgentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}