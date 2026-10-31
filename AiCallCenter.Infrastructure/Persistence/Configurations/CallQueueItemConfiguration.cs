using AiCallCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiCallCenter.Infrastructure.Persistence.Configurations;

public sealed class CallQueueItemConfiguration
    : IEntityTypeConfiguration<CallQueueItem>
{
    public void Configure(
        EntityTypeBuilder<CallQueueItem> builder)
    {
        builder.ToTable("CallQueueItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .HasConversion<int>();

        builder.Property(x => x.QueueName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.TransferReason)
            .HasMaxLength(500);

        builder.Property(x => x.AiHandoffSummary)
            .HasMaxLength(4000);

        builder.Property(x => x.Priority)
            .HasDefaultValue(1);

        builder.HasOne(x => x.Company)
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CallQueue)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.CallQueueId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Call)
            .WithMany()
            .HasForeignKey(x => x.CallId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.AssignedOperator)
            .WithMany()
            .HasForeignKey(x => x.AssignedOperatorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.CompanyId);

        builder.HasIndex(x => x.CallQueueId);

        builder.HasIndex(x => x.CallId);

        builder.HasIndex(x => x.AssignedOperatorId);

        builder.HasIndex(x => x.Status);

        builder.HasIndex(x => x.WaitingSince);

        builder.HasIndex(x => new
        {
            x.CompanyId,
            x.CallQueueId,
            x.Status,
            x.Priority,
            x.WaitingSince
        });
    }
}