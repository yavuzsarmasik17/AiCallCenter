using AiCallCenter.Application.CallQueueItems.Dtos;
using MediatR;

namespace AiCallCenter.Application.CallQueueItems.Commands.CompleteQueuedCall;

public sealed record CompleteQueuedCallCommand(
    Guid CompanyId,
    Guid QueueItemId,
    Guid CallOperatorId,
    string? Summary,
    string? Result)
    : IRequest<QueuedCallActionResultDto>;
