using AiCallCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiCallCenter.Infrastructure.Configurations;

public sealed class AiAgentKnowledgeBaseConfiguration
    : IEntityTypeConfiguration<AiAgentKnowledgeBase>
{
    public void Configure(
        EntityTypeBuilder<AiAgentKnowledgeBase> builder)
    {
        builder.ToTable("AiAgentKnowledgeBases");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.AiAgentId)
            .IsRequired();

        builder.Property(x => x.KnowledgeBaseId)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.AiAgentId,
            x.KnowledgeBaseId
        })
        .IsUnique();

        builder.HasOne(x => x.AiAgent)
            .WithMany(x => x.KnowledgeBases)
            .HasForeignKey(x => x.AiAgentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.KnowledgeBase)
            .WithMany(x => x.AiAgents)
            .HasForeignKey(x => x.KnowledgeBaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}