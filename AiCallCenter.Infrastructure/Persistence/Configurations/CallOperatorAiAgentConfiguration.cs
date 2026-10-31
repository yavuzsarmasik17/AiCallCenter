using AiCallCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiCallCenter.Infrastructure.Persistence.Configurations;

public sealed class CallOperatorAiAgentConfiguration
    : IEntityTypeConfiguration<CallOperatorAiAgent>
{
    public void Configure(
        EntityTypeBuilder<CallOperatorAiAgent> builder)
    {
        builder.ToTable("CallOperatorAiAgents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CompanyId)
            .IsRequired();

        builder.Property(x => x.CallOperatorId)
            .IsRequired();

        builder.Property(x => x.AiAgentId)
            .IsRequired();

        builder.Property(x => x.IsPrimary)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.AssignedAt)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.CallOperatorId,
            x.AiAgentId
        })
        .IsUnique();

        builder.HasIndex(x => new
        {
            x.CompanyId,
            x.CallOperatorId,
            x.IsActive
        });

        builder.HasIndex(x => new
        {
            x.CallOperatorId,
            x.IsPrimary
        });

        builder.HasOne(x => x.Company)
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CallOperator)
            .WithMany(x => x.AiAgentAssignments)
            .HasForeignKey(x => x.CallOperatorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.AiAgent)
            .WithMany(x => x.OperatorAssignments)
            .HasForeignKey(x => x.AiAgentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}