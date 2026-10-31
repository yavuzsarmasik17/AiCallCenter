using AiCallCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiCallCenter.Infrastructure.Configurations;

public sealed class CallMessageConfiguration
    : IEntityTypeConfiguration<CallMessage>
{
    public void Configure(EntityTypeBuilder<CallMessage> builder)
    {
        builder.ToTable("CallMessages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.CallId)
            .IsRequired();

        builder.Property(x => x.Speaker)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Content)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.SpokenAt)
            .IsRequired();

        builder.Property(x => x.SequenceNumber)
            .IsRequired();

        builder.Property(x => x.ConfidenceScore)
            .HasPrecision(5, 4);

        builder.Property(x => x.AudioFileUrl)
            .HasMaxLength(2000);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .IsRequired();

        builder.HasIndex(x => x.CallId);

        builder.HasIndex(x => new
        {
            x.CallId,
            x.SequenceNumber
        })
        .IsUnique();

        builder.HasOne(x => x.Call)
            .WithMany(x => x.Messages)
            .HasForeignKey(x => x.CallId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}