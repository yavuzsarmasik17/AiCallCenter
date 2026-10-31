using AiCallCenter.Application.Authentication.Interfaces;
using AiCallCenter.Application.Common.PasswordReset;
using AiCallCenter.Application.Common.Security;
using AiCallCenter.Application.Users.Interfaces;
using MediatR;

namespace AiCallCenter.Application.Authentication.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandler
    : IRequestHandler<ResetPasswordCommand>
{
    private readonly IUserRepository _userRepository;

    private readonly IPasswordResetCodeRepository
        _passwordResetCodeRepository;

    private readonly IPasswordResetCodeGenerator
        _codeGenerator;

    private readonly IPasswordHasher _passwordHasher;

    public ResetPasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordResetCodeRepository passwordResetCodeRepository,
        IPasswordResetCodeGenerator codeGenerator,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordResetCodeRepository =
            passwordResetCodeRepository;
        _codeGenerator = codeGenerator;
        _passwordHasher = passwordHasher;
    }

    public async Task Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        ValidateRequest(request);

        var normalizedEmail = request.Email
            .Trim()
            .ToLowerInvariant();

        var user = await _userRepository.GetByEmailAsync(
            normalizedEmail,
            cancellationToken);

        if (user is null || !user.IsActive)
        {
            throw new InvalidOperationException(
                "Şifre sıfırlama işlemi gerçekleştirilemedi.");
        }

        var passwordResetCode =
            await _passwordResetCodeRepository
                .GetLatestActiveByUserIdAsync(
                    user.Id,
                    cancellationToken);

        if (passwordResetCode is null ||
            !passwordResetCode.CanBeUsed())
        {
            throw new InvalidOperationException(
                "Doğrulama kodunun süresi dolmuş veya kod geçersiz.");
        }

        var isCodeValid = _codeGenerator.Verify(
            request.Code.Trim(),
            passwordResetCode.CodeHash);

        if (!isCodeValid)
        {
            passwordResetCode.RegisterFailedAttempt();

            await _passwordResetCodeRepository
                .SaveChangesAsync(cancellationToken);

            throw new InvalidOperationException(
                "Doğrulama kodu hatalı.");
        }

        var newPasswordHash =
            _passwordHasher.Hash(request.NewPassword);

        user.ChangePassword(newPasswordHash);

        passwordResetCode.MarkAsUsed();

        await _passwordResetCodeRepository
            .SaveChangesAsync(cancellationToken);
    }

    private static void ValidateRequest(
        ResetPasswordCommand request)
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

        if (string.IsNullOrWhiteSpace(request.NewPassword))
        {
            throw new ArgumentException(
                "Yeni parola boş olamaz.");
        }

        if (request.NewPassword.Length < 8)
        {
            throw new ArgumentException(
                "Yeni parola en az 8 karakter olmalıdır.");
        }

        if (request.NewPassword != request.ConfirmPassword)
        {
            throw new ArgumentException(
                "Yeni parola ile parola tekrarı eşleşmiyor.");
        }
    }
}