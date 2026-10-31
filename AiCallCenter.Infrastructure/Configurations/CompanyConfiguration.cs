using AiCallCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiCallCenter.Infrastructure.Configurations;

public sealed class CompanyConfiguration
    : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.TaxNumber)
            .HasMaxLength(50);

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(30);

        builder.Property(x => x.Email)
            .HasMaxLength(320);

        builder.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => x.Slug)
            .IsUnique();

        builder.Property(x => x.SubscriptionPlan)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.MaxUserCount)
            .IsRequired();

        builder.Property(x => x.MaxAiAgentCount)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .IsRequired();

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}