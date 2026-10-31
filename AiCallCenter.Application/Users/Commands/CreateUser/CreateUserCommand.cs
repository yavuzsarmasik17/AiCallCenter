using AiCallCenter.Application.Users.DTOs;
using AiCallCenter.Domain.Enums;
using MediatR;

namespace AiCallCenter.Application.Users.Commands.CreateUser;

public sealed record CreateUserCommand(
    Guid CompanyId,
    string FirstName,
    string LastName,
    string Email,
    string Password,
    UserRole Role)
    : IRequest<UserDto>;