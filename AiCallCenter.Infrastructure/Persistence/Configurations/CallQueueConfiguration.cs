using AiCallCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiCallCenter.Infrastructure.Persistence.Configurations;

public sealed class CallQueueConfiguration
    : IEntityTypeConfiguration<CallQueue>
{
    public void Configure(
        EntityTypeBuilder<CallQueue> builder)
    {
        builder.ToTable("CallQueues");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.Priority)
            .HasDefaultValue(1)
            .IsRequired();

        builder.Property(x => x.MaxWaitingSeconds)
            .HasDefaultValue(300)
            .IsRequired();

        builder.Property(x => x.MaxQueueSize)
            .HasDefaultValue(100)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.CompanyId,
            x.Name
        })
        .IsUnique();

        builder.HasIndex(x => new
        {
            x.CompanyId,
            x.IsActive
        });

        builder.HasOne(x => x.Company)
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AiAgent)
            .WithMany()
            .HasForeignKey(x => x.AiAgentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}