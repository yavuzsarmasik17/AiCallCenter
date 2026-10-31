using AiCallCenter.Application.CallQueueItems.Dtos;
using AiCallCenter.Application.CallQueueItems.Interfaces;
using AiCallCenter.Application.CallQueueItems.Mappings;
using MediatR;

namespace AiCallCenter.Application.CallQueueItems.Queries.GetOperatorCallQueueItems;

public sealed class GetOperatorCallQueueItemsQueryHandler
    : IRequestHandler<GetOperatorCallQueueItemsQuery, IReadOnlyList<CallQueueItemListDto>>
{
    private readonly ICallQueueItemRepository _repository;

    public GetOperatorCallQueueItemsQueryHandler(ICallQueueItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<CallQueueItemListDto>> Handle(
        GetOperatorCallQueueItemsQuery request,
        CancellationToken cancellationToken)
    {
        var items = await _repository.GetByOperatorAsync(
            request.CompanyId,
            request.OperatorId,
            request.ActiveOnly,
            cancellationToken);

        return items.Select(x => x.ToListDto()).ToList();
    }
}
