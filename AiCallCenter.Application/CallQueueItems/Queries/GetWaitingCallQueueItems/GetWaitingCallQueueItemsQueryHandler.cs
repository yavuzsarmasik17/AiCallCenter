using AiCallCenter.Application.CallQueueItems.Dtos;
using AiCallCenter.Application.CallQueueItems.Interfaces;
using AiCallCenter.Application.CallQueueItems.Mappings;
using MediatR;

namespace AiCallCenter.Application.CallQueueItems.Queries.GetWaitingCallQueueItems;

public sealed class GetWaitingCallQueueItemsQueryHandler
    : IRequestHandler<GetWaitingCallQueueItemsQuery, IReadOnlyList<CallQueueItemListDto>>
{
    private readonly ICallQueueItemRepository _repository;

    public GetWaitingCallQueueItemsQueryHandler(ICallQueueItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<CallQueueItemListDto>> Handle(
        GetWaitingCallQueueItemsQuery request,
        CancellationToken cancellationToken)
    {
        var items = await _repository.GetWaitingAsync(
            request.CompanyId,
            request.CallQueueId,
            cancellationToken);

        return items.Select(x => x.ToListDto()).ToList();
    }
}
