using MediatR;

namespace AiCallCenter.Application.Users.Commands.ChangeUserPassword;

public sealed record ChangeUserPasswordCommand(
    Guid CompanyId,
    Guid UserId,
    string NewPassword) : IRequest;
