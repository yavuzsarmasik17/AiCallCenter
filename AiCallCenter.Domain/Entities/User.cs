using AiCallCenter.Domain.Common;
using AiCallCenter.Domain.Enums;

namespace AiCallCenter.Domain.Entities;

public sealed class User : AuditableEntity
{
    public Guid CompanyId { get; private set; }

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public UserRole Role { get; private set; }

    public bool IsActive { get; private set; }

    public bool IsDeleted { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    public Guid? DeletedByUserId { get; private set; }

    public DateTime? LastLoginAt { get; private set; }

    public Company Company { get; private set; } = null!;

    public ICollection<PasswordResetCode> PasswordResetCodes
    { get; private set; }
        = new List<PasswordResetCode>();

    public string FullName => $"{FirstName} {LastName}".Trim();

    private User()
    {
    }

    public User(
        Guid companyId,
        string firstName,
        string lastName,
        string email,
        string passwordHash,
        UserRole role)
    {
        if (companyId == Guid.Empty)
        {
            throw new ArgumentException(
                "Firma kimliği geçersiz.",
                nameof(companyId));
        }

        CompanyId = companyId;

        SetName(firstName, lastName);
        SetEmail(email);
        SetPasswordHash(passwordHash);

        Role = role;
        IsActive = true;
        IsDeleted = false;
    }

    public void UpdateProfile(
        string firstName,
        string lastName,
        string email)
    {
        SetName(firstName, lastName);
        SetEmail(email);
    }

    public void ChangePassword(string passwordHash)
    {
        SetPasswordHash(passwordHash);
    }

    public void ChangeRole(UserRole role)
    {
        Role = role;
    }

    public void MarkLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException(
                "Silinmiş kullanıcı tekrar aktifleştirilemez.");
        }

        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Delete(Guid deletedByUserId)
    {
        if (deletedByUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "Silen kullanıcı kimliği geçersiz.",
                nameof(deletedByUserId));
        }

        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        IsActive = false;
        DeletedAt = DateTime.UtcNow;
        DeletedByUserId = deletedByUserId;
    }

    private void SetName(
        string firstName,
        string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new ArgumentException(
                "Kullanıcı adı boş olamaz.",
                nameof(firstName));
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new ArgumentException(
                "Kullanıcı soyadı boş olamaz.",
                nameof(lastName));
        }

        if (firstName.Trim().Length > 100)
        {
            throw new ArgumentException(
                "Kullanıcı adı en fazla 100 karakter olabilir.",
                nameof(firstName));
        }

        if (lastName.Trim().Length > 100)
        {
            throw new ArgumentException(
                "Kullanıcı soyadı en fazla 100 karakter olabilir.",
                nameof(lastName));
        }

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
    }

    private void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException(
                "E-posta adresi boş olamaz.",
                nameof(email));
        }

        var normalizedEmail = email
            .Trim()
            .ToLowerInvariant();

        if (normalizedEmail.Length > 320)
        {
            throw new ArgumentException(
                "E-posta adresi en fazla 320 karakter olabilir.",
                nameof(email));
        }

        Email = normalizedEmail;
    }

    private void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException(
                "Parola özeti boş olamaz.",
                nameof(passwordHash));
        }

        PasswordHash = passwordHash.Trim();
    }
}
