using MediatR;

namespace AiCallCenter.Application.Authentication.Commands.ResetPassword;

public sealed record ResetPasswordCommand(
    string Email,
    string Code,
    string NewPassword,
    string ConfirmPassword)
    : IRequest;