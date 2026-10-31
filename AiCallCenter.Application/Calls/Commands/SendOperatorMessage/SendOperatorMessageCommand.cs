using AiCallCenter.Application.Calls.Dtos;
using MediatR;

namespace AiCallCenter.Application.Calls.Commands.SendOperatorMessage;

public sealed record SendOperatorMessageCommand(
    Guid CompanyId,
    Guid CallId,
    string Message)
    : IRequest<CallMessageDto>;