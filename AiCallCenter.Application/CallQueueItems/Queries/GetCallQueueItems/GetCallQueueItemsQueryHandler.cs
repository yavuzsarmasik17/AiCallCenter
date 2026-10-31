using AiCallCenter.Application.CallQueueItems.Dtos;
using AiCallCenter.Application.CallQueueItems.Interfaces;
using AiCallCenter.Application.CallQueueItems.Mappings;
using MediatR;

namespace AiCallCenter.Application.CallQueueItems.Queries.GetCallQueueItems;

public sealed class GetCallQueueItemsQueryHandler
    : IRequestHandler<GetCallQueueItemsQuery, IReadOnlyList<CallQueueItemListDto>>
{
    private readonly ICallQueueItemRepository _repository;

    public GetCallQueueItemsQueryHandler(ICallQueueItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<CallQueueItemListDto>> Handle(
        GetCallQueueItemsQuery request,
        CancellationToken cancellationToken)
    {
        var items = await _repository.GetAllAsync(
            request.CompanyId,
            request.Status,
            request.CallQueueId,
            cancellationToken);

        return items.Select(x => x.ToListDto()).ToList();
    }
}
