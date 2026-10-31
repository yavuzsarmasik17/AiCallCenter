using AiCallCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiCallCenter.Infrastructure.Configurations;

public sealed class KnowledgeDocumentConfiguration
    : IEntityTypeConfiguration<KnowledgeDocument>
{
    public void Configure(
        EntityTypeBuilder<KnowledgeDocument> builder)
    {
        builder.ToTable("KnowledgeDocuments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.KnowledgeBaseId)
            .IsRequired();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(x => x.OriginalFileName)
            .HasMaxLength(500);

        builder.Property(x => x.FilePath)
            .HasMaxLength(1000);

        builder.Property(x => x.MimeType)
            .HasMaxLength(200);

        builder.Property(x => x.FileSizeBytes)
            .IsRequired();

        builder.Property(x => x.DocumentType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(4000);

        builder.Property(x => x.UploadedAt)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .IsRequired();

        builder.HasIndex(x => x.KnowledgeBaseId);

        builder.HasIndex(x => x.Status);

        builder.HasOne(x => x.KnowledgeBase)
            .WithMany(x => x.Documents)
            .HasForeignKey(x => x.KnowledgeBaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}