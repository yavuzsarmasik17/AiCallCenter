using AiCallCenter.Application.CallQueueItems.Dtos;
using MediatR;

namespace AiCallCenter.Application.CallQueueItems.Queries.GetWaitingCallQueueItems;

public sealed record GetWaitingCallQueueItemsQuery(
    Guid CompanyId,
    Guid? CallQueueId)
    : IRequest<IReadOnlyList<CallQueueItemListDto>>;
