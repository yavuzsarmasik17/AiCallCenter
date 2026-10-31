using AiCallCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiCallCenter.Infrastructure.Configurations;

public sealed class PasswordResetCodeConfiguration
    : IEntityTypeConfiguration<PasswordResetCode>
{
    public void Configure(
        EntityTypeBuilder<PasswordResetCode> builder)
    {
        builder.ToTable("PasswordResetCodes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CodeHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.Property(x => x.FailedAttemptCount)
            .HasDefaultValue(0);

        builder.Property(x => x.IsUsed)
            .HasDefaultValue(false);

        builder.HasOne(x => x.User)
            .WithMany(x => x.PasswordResetCodes)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.UserId);

        builder.HasIndex(x => x.ExpiresAt);

        builder.HasIndex(x => x.IsUsed);
    }
}