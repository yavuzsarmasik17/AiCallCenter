using AiCallCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiCallCenter.Infrastructure.Configurations;

public sealed class CallConfiguration
    : IEntityTypeConfiguration<Call>
{
    public void Configure(EntityTypeBuilder<Call> builder)
    {
        builder.ToTable("Calls");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.CompanyId)
            .IsRequired();

        builder.Property(x => x.AiAgentId)
            .IsRequired();

        builder.Property(x => x.ProviderCallId)
            .HasMaxLength(200);

        builder.Property(x => x.CallerNumber)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(x => x.CalledNumber)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(x => x.Direction)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.StartedAt)
            .IsRequired();

        builder.Property(x => x.DurationSeconds)
            .IsRequired();

        builder.Property(x => x.Summary)
            .HasMaxLength(10000);

        builder.Property(x => x.Result)
            .HasMaxLength(2000);

        builder.Property(x => x.TransferredToHuman)
            .IsRequired();

        builder.Property(x => x.TransferNumber)
            .HasMaxLength(30);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .IsRequired();

        builder.HasIndex(x => x.CompanyId);

        builder.HasIndex(x => x.AiAgentId);

        builder.HasIndex(x => x.ProviderCallId);

        builder.HasIndex(x => x.CallerNumber);

        builder.HasIndex(x => x.StartedAt);

        builder.HasOne(x => x.Company)
            .WithMany(x => x.Calls)
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AiAgent)
            .WithMany(x => x.Calls)
            .HasForeignKey(x => x.AiAgentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}