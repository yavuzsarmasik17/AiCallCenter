using AiCallCenter.Application.Users.DTOs;
using MediatR;

namespace AiCallCenter.Application.Users.Commands.SetUserActiveStatus;

public sealed record SetUserActiveStatusCommand(
    Guid CompanyId,
    Guid UserId,
    bool IsActive) : IRequest<UserDto>;
