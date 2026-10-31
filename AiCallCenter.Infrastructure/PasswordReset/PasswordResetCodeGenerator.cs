using System.Security.Cryptography;
using System.Text;
using AiCallCenter.Application.Common.PasswordReset;

namespace AiCallCenter.Infrastructure.PasswordReset;

public sealed class PasswordResetCodeGenerator
    : IPasswordResetCodeGenerator
{
    public string GenerateCode()
    {
        return RandomNumberGenerator
            .GetInt32(100000, 999999)
            .ToString();
    }

    public string HashCode(string code)
    {
        using var sha = SHA256.Create();

        var bytes =
            Encoding.UTF8.GetBytes(code);

        return Convert.ToHexString(
            sha.ComputeHash(bytes));
    }

    public bool Verify(
        string code,
        string hash)
    {
        return HashCode(code) == hash;
    }
}