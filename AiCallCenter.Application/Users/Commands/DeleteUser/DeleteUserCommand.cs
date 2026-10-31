using MediatR;

namespace AiCallCenter.Application.Users.Commands.DeleteUser;

public sealed record DeleteUserCommand(
    Guid CompanyId,
    Guid UserId,
    Guid DeletedByUserId)
    : IRequest;
