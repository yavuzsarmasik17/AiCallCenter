using AiCallCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiCallCenter.Infrastructure.Persistence.Configurations;

public sealed class CallOperatorConfiguration
    : IEntityTypeConfiguration<CallOperator>
{
    public void Configure(EntityTypeBuilder<CallOperator> builder)
    {
        builder.ToTable("CallOperators");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DisplayName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.ExtensionNumber)
            .HasMaxLength(20);

        builder.Property(x => x.Status)
            .HasConversion<int>();

        builder.Property(x => x.MaxConcurrentCalls)
            .HasDefaultValue(1);

        builder.Property(x => x.ActiveCallCount)
            .HasDefaultValue(0);

        builder.Property(x => x.CanReceiveCalls)
            .HasDefaultValue(true);

        builder.HasOne(x => x.Company)
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.CompanyId);

        builder.HasIndex(x => x.UserId);

        builder.HasIndex(x => x.Status);

        builder.HasIndex(x => new
        {
            x.CompanyId,
            x.Status
        });
    }
}