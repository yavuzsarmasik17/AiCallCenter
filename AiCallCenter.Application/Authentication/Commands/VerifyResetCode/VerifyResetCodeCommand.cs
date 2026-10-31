using MediatR;

namespace AiCallCenter.Application.Authentication.Commands.VerifyResetCode;

public sealed record VerifyResetCodeCommand(
    string Email,
    string Code)
    : IRequest<bool>;