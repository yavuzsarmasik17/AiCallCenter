using AiCallCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiCallCenter.Infrastructure.Persistence.Configurations;

public sealed class CallQueueOperatorConfiguration
    : IEntityTypeConfiguration<CallQueueOperator>
{
    public void Configure(
        EntityTypeBuilder<CallQueueOperator> builder)
    {
        builder.ToTable("CallQueueOperators");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Priority)
            .HasDefaultValue(1)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(x => x.AssignedAt)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.CallQueueId,
            x.CallOperatorId
        })
        .IsUnique();

        builder.HasIndex(x => new
        {
            x.CompanyId,
            x.CallQueueId,
            x.IsActive
        });

        builder.HasOne(x => x.Company)
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CallQueue)
            .WithMany(x => x.OperatorAssignments)
            .HasForeignKey(x => x.CallQueueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.CallOperator)
            .WithMany(x => x.QueueAssignments)
            .HasForeignKey(x => x.CallOperatorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}