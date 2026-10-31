using AiCallCenter.Application.Authentication.Interfaces;
using AiCallCenter.Application.Common.PasswordReset;
using AiCallCenter.Application.Users.Interfaces;
using MediatR;

namespace AiCallCenter.Application.Authentication.Commands.VerifyResetCode;

public sealed class VerifyResetCodeCommandHandler
    : IRequestHandler<VerifyResetCodeCommand, bool>
{
    private readonly IUserRepository _userRepository;

    private readonly IPasswordResetCodeRepository
        _passwordResetCodeRepository;

    private readonly IPasswordResetCodeGenerator
        _codeGenerator;

    public VerifyResetCodeCommandHandler(
        IUserRepository userRepository,
        IPasswordResetCodeRepository passwordResetCodeRepository,
        IPasswordResetCodeGenerator codeGenerator)
    {
        _userRepository = userRepository;
        _passwordResetCodeRepository =
            passwordResetCodeRepository;
        _codeGenerator = codeGenerator;
    }

    public async Task<bool> Handle(
        VerifyResetCodeCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException(
                "E-posta adresi boş olamaz.");
        }

        if (string.IsNullOrWhiteSpace(request.Code))
        {
            throw new ArgumentException(
                "Doğrulama kodu boş olamaz.");
        }

        var code = request.Code.Trim();

        if (code.Length != 6 ||
            !code.All(char.IsDigit))
        {
            throw new ArgumentException(
                "Doğrulama kodu 6 haneli olmalıdır.");
        }

        var normalizedEmail = request.Email
            .Trim()
            .ToLowerInvariant();

        var user = await _userRepository.GetByEmailAsync(
            normalizedEmail,
            cancellationToken);

        if (user is null || !user.IsActive)
        {
            return false;
        }

        var passwordResetCode =
            await _passwordResetCodeRepository
                .GetLatestActiveByUserIdAsync(
                    user.Id,
                    cancellationToken);

        if (passwordResetCode is null)
        {
            return false;
        }

        if (!passwordResetCode.CanBeUsed())
        {
            return false;
        }

        var isValid = _codeGenerator.Verify(
            code,
            passwordResetCode.CodeHash);

        if (!isValid)
        {
            passwordResetCode.RegisterFailedAttempt();

            await _passwordResetCodeRepository
                .SaveChangesAsync(cancellationToken);

            return false;
        }

        return true;
    }
}