using AiCallCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiCallCenter.Infrastructure.Configurations;

public sealed class KnowledgeBaseConfiguration
    : IEntityTypeConfiguration<KnowledgeBase>
{
    public void Configure(
        EntityTypeBuilder<KnowledgeBase> builder)
    {
        builder.ToTable("KnowledgeBases");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.CompanyId)
            .IsRequired();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .IsRequired();

        builder.HasIndex(x => x.CompanyId);

        builder.HasIndex(x => new
        {
            x.CompanyId,
            x.Name
        });

        builder.HasOne(x => x.Company)
            .WithMany(x => x.KnowledgeBases)
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}