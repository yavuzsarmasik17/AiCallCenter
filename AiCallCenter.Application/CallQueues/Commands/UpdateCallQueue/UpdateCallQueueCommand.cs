using AiCallCenter.Application.CallQueues.Dtos;
using MediatR;

namespace AiCallCenter.Application.CallQueues.Commands
    .UpdateCallQueue;

public sealed record UpdateCallQueueCommand(
    Guid CompanyId,
    Guid CallQueueId,
    string Name,
    string? Description,
    int Priority,
    int MaxWaitingSeconds,
    int MaxQueueSize,
    Guid? AiAgentId,
    bool IsActive)
    : IRequest<CallQueueDto>;