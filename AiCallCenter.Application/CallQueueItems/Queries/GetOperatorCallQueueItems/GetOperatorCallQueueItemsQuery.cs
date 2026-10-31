using AiCallCenter.Application.CallQueueItems.Dtos;
using MediatR;

namespace AiCallCenter.Application.CallQueueItems.Queries.GetOperatorCallQueueItems;

public sealed record GetOperatorCallQueueItemsQuery(
    Guid CompanyId,
    Guid OperatorId,
    bool ActiveOnly)
    : IRequest<IReadOnlyList<CallQueueItemListDto>>;
