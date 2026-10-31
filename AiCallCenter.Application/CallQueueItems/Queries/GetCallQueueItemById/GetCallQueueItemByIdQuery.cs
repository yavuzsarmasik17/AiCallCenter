using AiCallCenter.Application.CallQueueItems.Dtos;
using MediatR;

namespace AiCallCenter.Application.CallQueueItems.Queries.GetCallQueueItemById;

public sealed record GetCallQueueItemByIdQuery(
    Guid CompanyId,
    Guid QueueItemId)
    : IRequest<CallQueueItemListDto>;
