using AiCallCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiCallCenter.Infrastructure.Persistence.Configurations;

public sealed class CallEventConfiguration
    : IEntityTypeConfiguration<CallEvent>
{
    public void Configure(EntityTypeBuilder<CallEvent> builder)
    {
        builder.ToTable("CallEvents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventType)
            .HasConversion<int>();

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.MetadataJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.OccurredAt)
            .IsRequired();

        builder.HasOne(x => x.Company)
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Call)
     .WithMany(x => x.Events)
             .HasForeignKey(x => x.CallId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Operator)
            .WithMany()
            .HasForeignKey(x => x.OperatorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AiAgent)
            .WithMany()
            .HasForeignKey(x => x.AiAgentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.CompanyId);

        builder.HasIndex(x => x.CallId);

        builder.HasIndex(x => x.OperatorId);

        builder.HasIndex(x => x.AiAgentId);

        builder.HasIndex(x => x.EventType);

        builder.HasIndex(x => x.OccurredAt);

        builder.HasIndex(x => new
        {
            x.CallId,
            x.OccurredAt
        });
    }
}