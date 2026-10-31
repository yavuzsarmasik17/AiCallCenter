using AiCallCenter.Application.CallQueues.Dtos;
using MediatR;

namespace AiCallCenter.Application.CallQueues.Commands
    .AssignOperatorToQueue;

public sealed record AssignOperatorToQueueCommand(
    Guid CompanyId,
    Guid CallQueueId,
    Guid CallOperatorId,
    int Priority)
    : IRequest<CallQueueOperatorDto>;