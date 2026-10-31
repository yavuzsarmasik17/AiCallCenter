namespace AiCallCenter.Application.Common.PasswordReset;

public interface IPasswordResetCodeGenerator
{
    string GenerateCode();

    string HashCode(string code);

    bool Verify(
        string code,
        string hash);
}