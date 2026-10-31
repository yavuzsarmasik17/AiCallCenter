using AiCallCenter.Application.CallQueueItems.Dtos;
using AiCallCenter.Domain.Enums;
using MediatR;

namespace AiCallCenter.Application.CallQueueItems.Queries.GetCallQueueItems;

public sealed record GetCallQueueItemsQuery(
    Guid CompanyId,
    CallQueueStatus? Status,
    Guid? CallQueueId)
    : IRequest<IReadOnlyList<CallQueueItemListDto>>;
