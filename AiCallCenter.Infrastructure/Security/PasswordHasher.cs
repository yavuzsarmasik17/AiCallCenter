using AiCallCenter.Application.Common.Security;
using Microsoft.AspNetCore.Identity;

namespace AiCallCenter.Infrastructure.Security;

public sealed class PasswordHasher
    : IPasswordHasher
{
    private readonly PasswordHasher<object> _hasher = new();

    public string Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException(
                "Parola boş olamaz.",
                nameof(password));
        }

        return _hasher.HashPassword(
            new object(),
            password);
    }

    public bool Verify(
        string password,
        string passwordHash)
    {
        var result = _hasher.VerifyHashedPassword(
            new object(),
            passwordHash,
            password);

        return result != PasswordVerificationResult.Failed;
    }
}