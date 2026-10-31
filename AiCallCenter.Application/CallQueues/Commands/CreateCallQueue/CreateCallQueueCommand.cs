using AiCallCenter.Application.CallQueues.Dtos;
using MediatR;

namespace AiCallCenter.Application.CallQueues.Commands
    .CreateCallQueue;

public sealed record CreateCallQueueCommand(
    Guid CompanyId,
    string Name,
    string? Description,
    int Priority,
    int MaxWaitingSeconds,
    int MaxQueueSize,
    Guid? AiAgentId)
    : IRequest<CallQueueDto>;