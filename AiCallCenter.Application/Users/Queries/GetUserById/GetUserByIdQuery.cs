using AiCallCenter.Application.Users.DTOs;
using MediatR;

namespace AiCallCenter.Application.Users.Queries.GetUserById;

public sealed record GetUserByIdQuery(
    Guid CompanyId,
    Guid UserId) : IRequest<UserDto>;
