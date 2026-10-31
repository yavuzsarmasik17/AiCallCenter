using AiCallCenter.Application.Authentication.Interfaces;
using AiCallCenter.Application.Common.Email;
using AiCallCenter.Application.Common.PasswordReset;
using AiCallCenter.Application.Users.Interfaces;
using AiCallCenter.Domain.Entities;
using MediatR;

namespace AiCallCenter.Application.Authentication.Commands.ForgotPassword;

public sealed class ForgotPasswordCommandHandler
    : IRequestHandler<ForgotPasswordCommand>
{
    private readonly IUserRepository _userRepository;

    private readonly IPasswordResetCodeRepository
        _passwordResetCodeRepository;

    private readonly IPasswordResetCodeGenerator
        _codeGenerator;

    private readonly IEmailSender _emailSender;

    public ForgotPasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordResetCodeRepository passwordResetCodeRepository,
        IPasswordResetCodeGenerator codeGenerator,
        IEmailSender emailSender)
    {
        _userRepository = userRepository;
        _passwordResetCodeRepository =
            passwordResetCodeRepository;
        _codeGenerator = codeGenerator;
        _emailSender = emailSender;
    }

    public async Task Handle(
        ForgotPasswordCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException(
                "E-posta adresi boş olamaz.");
        }

        var normalizedEmail = request.Email
            .Trim()
            .ToLowerInvariant();

        var user = await _userRepository.GetByEmailAsync(
            normalizedEmail,
            cancellationToken);

        /*
         * Güvenlik nedeniyle kullanıcı bulunamadığında hata vermiyoruz.
         * Böylece dışarıdan hangi e-posta adreslerinin sistemde kayıtlı
         * olduğu anlaşılamaz.
         */
        if (user is null || !user.IsActive)
        {
            return;
        }

        var activeCodes =
            await _passwordResetCodeRepository
                .GetActiveCodesByUserIdAsync(
                    user.Id,
                    cancellationToken);

        foreach (var activeCode in activeCodes)
        {
            activeCode.MarkAsUsed();
        }

        var verificationCode =
            _codeGenerator.GenerateCode();

        var codeHash =
            _codeGenerator.HashCode(
                verificationCode);

        var passwordResetCode =
            new PasswordResetCode(
                user.Id,
                codeHash,
                DateTime.UtcNow.AddMinutes(10));

        await _passwordResetCodeRepository.AddAsync(
            passwordResetCode,
            cancellationToken);

        await _passwordResetCodeRepository.SaveChangesAsync(
            cancellationToken);

        var subject =
            "AI Call Center - Şifre Sıfırlama Kodu";

        var htmlBody = CreateEmailBody(
            user.FullName,
            verificationCode);

        try
        {
            await _emailSender.SendAsync(
                user.Email,
                subject,
                htmlBody,
                cancellationToken);
        }
        catch
        {
            // E-posta gönderilemediyse oluşturulan kodu geçersiz yap.
            passwordResetCode.MarkAsUsed();

            await _passwordResetCodeRepository.SaveChangesAsync(
                cancellationToken);

            throw new InvalidOperationException(
                "Şifre sıfırlama e-postası gönderilemedi.");
        }
    }

    private static string CreateEmailBody(
        string fullName,
        string verificationCode)
    {
        var safeName = System.Net.WebUtility.HtmlEncode(
            string.IsNullOrWhiteSpace(fullName)
                ? "Kullanıcı"
                : fullName);

        var safeCode = System.Net.WebUtility.HtmlEncode(
            verificationCode);

        return $$"""
        <!DOCTYPE html>
        <html lang="tr">
        <head>
            <meta charset="UTF-8">
            <meta name="viewport"
                  content="width=device-width, initial-scale=1.0">
        </head>

        <body style="
            margin:0;
            padding:0;
            background-color:#f4f6f8;
            font-family:Arial,Helvetica,sans-serif;">

            <table width="100%"
                   cellpadding="0"
                   cellspacing="0"
                   style="padding:32px 16px;">

                <tr>
                    <td align="center">

                        <table width="100%"
                               cellpadding="0"
                               cellspacing="0"
                               style="
                                   max-width:560px;
                                   background:#ffffff;
                                   border-radius:12px;
                                   overflow:hidden;
                                   box-shadow:0 4px 18px rgba(0,0,0,0.08);">

                            <tr>
                                <td style="
                                    padding:24px;
                                    background:#172554;
                                    color:#ffffff;
                                    text-align:center;">

                                    <h1 style="
                                        margin:0;
                                        font-size:24px;">

                                        AI Call Center
                                    </h1>
                                </td>
                            </tr>

                            <tr>
                                <td style="
                                    padding:32px;
                                    color:#1f2937;">

                                    <h2 style="
                                        margin-top:0;
                                        font-size:21px;">

                                        Şifre sıfırlama isteği
                                    </h2>

                                    <p style="
                                        margin:16px 0;
                                        line-height:1.6;">

                                        Merhaba {{safeName}},
                                    </p>

                                    <p style="
                                        margin:16px 0;
                                        line-height:1.6;">

                                        Şifrenizi sıfırlamak için aşağıdaki
                                        doğrulama kodunu kullanın:
                                    </p>

                                    <div style="
                                        margin:28px 0;
                                        padding:18px;
                                        background:#f1f5f9;
                                        border-radius:10px;
                                        text-align:center;
                                        font-size:32px;
                                        font-weight:bold;
                                        letter-spacing:8px;
                                        color:#172554;">

                                        {{safeCode}}
                                    </div>

                                    <p style="
                                        margin:16px 0;
                                        line-height:1.6;">

                                        Bu kod <strong>10 dakika</strong>
                                        geçerlidir ve yalnızca bir kez
                                        kullanılabilir.
                                    </p>

                                    <p style="
                                        margin:16px 0;
                                        line-height:1.6;
                                        color:#6b7280;">

                                        Bu işlemi siz başlatmadıysanız
                                        e-postayı dikkate almayabilirsiniz.
                                    </p>
                                </td>
                            </tr>

                            <tr>
                                <td style="
                                    padding:18px;
                                    background:#f8fafc;
                                    text-align:center;
                                    color:#64748b;
                                    font-size:13px;">

                                    AI Call Center Güvenlik Ekibi
                                </td>
                            </tr>
                        </table>

                    </td>
                </tr>
            </table>
        </body>
        </html>
        """;
    }
}