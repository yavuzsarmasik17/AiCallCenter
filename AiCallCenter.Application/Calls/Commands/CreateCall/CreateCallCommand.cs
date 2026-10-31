using AiCallCenter.Application.Calls.Dtos;
using AiCallCenter.Domain.Enums;
using MediatR;

namespace AiCallCenter.Application.Calls.Commands.CreateCall;

public sealed record CreateCallCommand(
    Guid CompanyId,
    Guid AiAgentId,
    string CallerNumber,
    string CalledNumber,
    CallDirection Direction)
    : IRequest<CallDto>;
