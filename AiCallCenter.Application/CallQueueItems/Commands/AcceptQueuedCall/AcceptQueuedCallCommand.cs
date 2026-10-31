using AiCallCenter.Application.CallQueueItems.Dtos;
using MediatR;

namespace AiCallCenter.Application.CallQueueItems.Commands.AcceptQueuedCall;

public sealed record AcceptQueuedCallCommand(
    Guid CompanyId,
    Guid QueueItemId,
    Guid CallOperatorId)
    : IRequest<QueuedCallActionResultDto>;
