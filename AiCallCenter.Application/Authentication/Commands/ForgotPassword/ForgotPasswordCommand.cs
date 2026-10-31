using MediatR;

namespace AiCallCenter.Application.Authentication.Commands.ForgotPassword;

public sealed record ForgotPasswordCommand(
    string Email)
    : IRequest;