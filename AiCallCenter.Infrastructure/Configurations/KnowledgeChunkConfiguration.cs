using AiCallCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiCallCenter.Infrastructure.Configurations;

public sealed class KnowledgeChunkConfiguration
    : IEntityTypeConfiguration<KnowledgeChunk>
{
    public void Configure(
        EntityTypeBuilder<KnowledgeChunk> builder)
    {
        builder.ToTable("KnowledgeChunks");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.KnowledgeDocumentId)
            .IsRequired();

        builder.Property(x => x.ChunkIndex)
            .IsRequired();

        builder.Property(x => x.Content)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.CharacterCount)
            .IsRequired();

        builder.Property(x => x.EmbeddingJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.EmbeddingModel)
            .HasMaxLength(200);

        builder.Property(x => x.EmbeddedAt);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .IsRequired();

        builder.HasIndex(x => x.KnowledgeDocumentId);

        builder.HasIndex(x => new
        {
            x.KnowledgeDocumentId,
            x.ChunkIndex
        })
        .IsUnique();

        builder.HasOne(x => x.KnowledgeDocument)
            .WithMany(x => x.Chunks)
            .HasForeignKey(x => x.KnowledgeDocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}