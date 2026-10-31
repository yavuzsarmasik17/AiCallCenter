using AiCallCenter.Application.Users.DTOs;
using AiCallCenter.Domain.Enums;
using MediatR;

namespace AiCallCenter.Application.Users.Commands.ChangeUserRole;

public sealed record ChangeUserRoleCommand(
    Guid CompanyId,
    Guid UserId,
    UserRole Role) : IRequest<UserDto>;
