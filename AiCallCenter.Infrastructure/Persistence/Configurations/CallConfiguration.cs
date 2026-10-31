using AiCallCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiCallCenter.Infrastructure.Persistence.Configurations;

public sealed class CallConfiguration
    : IEntityTypeConfiguration<Call>
{
    public void Configure(EntityTypeBuilder<Call> builder)
    {
        builder.ToTable("Calls");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProviderCallId)
            .HasMaxLength(100);

        builder.Property(x => x.CallerNumber)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.CalledNumber)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Direction)
            .HasConversion<int>();

        builder.Property(x => x.Status)
            .HasConversion<int>();

        builder.Property(x => x.Summary)
            .HasMaxLength(4000);

        builder.Property(x => x.Result)
            .HasMaxLength(1000);

        builder.Property(x => x.TransferNumber)
            .HasMaxLength(30);

        builder.HasMany(x => x.Messages)
            .WithOne(x => x.Call)
            .HasForeignKey(x => x.CallId);

        builder.HasOne(x => x.Company)
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AiAgent)
            .WithMany()
            .HasForeignKey(x => x.AiAgentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.CompanyId);

        builder.HasIndex(x => x.Status);

        builder.HasIndex(x => x.StartedAt);

        builder.HasIndex(x => x.ProviderCallId);
    }
}