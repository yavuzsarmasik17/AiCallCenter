using AiCallCenter.Application.Users.DTOs;
using MediatR;

namespace AiCallCenter.Application.Users.Commands.UpdateUser;

public sealed record UpdateUserCommand(
    Guid CompanyId,
    Guid UserId,
    string FirstName,
    string LastName,
    string Email) : IRequest<UserDto>;
