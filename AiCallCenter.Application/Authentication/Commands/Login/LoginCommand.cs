using AiCallCenter.Application.Authentication.DTOs;
using MediatR;

namespace AiCallCenter.Application.Authentication.Commands.Login;

public sealed record LoginCommand(
    string Email,
    string Password)
    : IRequest<LoginResponse>;