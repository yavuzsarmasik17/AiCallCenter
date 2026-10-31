using AiCallCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiCallCenter.Infrastructure.Configurations;

public sealed class AiAgentConfiguration
    : IEntityTypeConfiguration<AiAgent>
{
    public void Configure(EntityTypeBuilder<AiAgent> builder)
    {
        builder.ToTable("AiAgents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.CompanyId)
            .IsRequired();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Instructions)
            .IsRequired()
            .HasMaxLength(20000);

        builder.Property(x => x.WelcomeMessage)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.Language)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Provider)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.ModelName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.VoiceName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.MaxCallDurationMinutes)
            .IsRequired();

        builder.Property(x => x.TransferToHumanEnabled)
            .IsRequired();

        // AI ajanının insan temsilciye aktarım yaparken
        // kullanacağı varsayılan çağrı kuyruğu.
        // Mevcut ajanlarda değer olmayabileceği için nullable.
        builder.Property(x => x.TransferCallQueueId)
            .IsRequired(false);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .IsRequired();

        builder.HasIndex(x => x.CompanyId);

        builder.HasIndex(x => new
        {
            x.CompanyId,
            x.Name
        });

        builder.HasIndex(x => new
        {
            x.CompanyId,
            x.TransferCallQueueId
        });

        builder.HasOne(x => x.Company)
            .WithMany(x => x.AiAgents)
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}