using AiCallCenter.Application.Users.DTOs;
using MediatR;

namespace AiCallCenter.Application.Users.Queries.GetUsers;

public sealed record GetUsersQuery(Guid CompanyId)
    : IRequest<IReadOnlyList<UserDto>>;
