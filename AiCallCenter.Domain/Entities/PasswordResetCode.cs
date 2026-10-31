using AiCallCenter.Domain.Common;

namespace AiCallCenter.Domain.Entities;

public sealed class PasswordResetCode : AuditableEntity
{
    public Guid UserId { get; private set; }

    public string CodeHash { get; private set; } = string.Empty;

    public DateTime ExpiresAt { get; private set; }

    public int FailedAttemptCount { get; private set; }

    public bool IsUsed { get; private set; }

    public DateTime? UsedAt { get; private set; }

    public User User { get; private set; } = null!;

    private PasswordResetCode()
    {
    }

    public PasswordResetCode(
        Guid userId,
        string codeHash,
        DateTime expiresAt)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "Kullanıcı kimliği geçersiz.",
                nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(codeHash))
        {
            throw new ArgumentException(
                "Kod özeti boş olamaz.",
                nameof(codeHash));
        }

        if (expiresAt <= DateTime.UtcNow)
        {
            throw new ArgumentException(
                "Kodun geçerlilik süresi gelecekte olmalıdır.",
                nameof(expiresAt));
        }

        UserId = userId;
        CodeHash = codeHash.Trim();
        ExpiresAt = expiresAt;
        FailedAttemptCount = 0;
        IsUsed = false;
    }

    public bool IsExpired()
    {
        return DateTime.UtcNow >= ExpiresAt;
    }

    public bool CanBeUsed()
    {
        return !IsUsed
            && !IsExpired()
            && FailedAttemptCount < 5;
    }

    public void RegisterFailedAttempt()
    {
        if (IsUsed)
        {
            return;
        }

        FailedAttemptCount++;
    }

    public void MarkAsUsed()
    {
        if (IsUsed)
        {
            return;
        }

        IsUsed = true;
        UsedAt = DateTime.UtcNow;
    }

    public void Invalidate()
    {
        if (IsUsed)
        {
            return;
        }

        IsUsed = true;
        UsedAt = DateTime.UtcNow;
    }
}