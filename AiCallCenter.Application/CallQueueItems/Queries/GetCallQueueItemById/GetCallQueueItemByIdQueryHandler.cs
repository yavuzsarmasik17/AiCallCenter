using AiCallCenter.Application.CallQueueItems.Dtos;
using AiCallCenter.Application.CallQueueItems.Interfaces;
using AiCallCenter.Application.CallQueueItems.Mappings;
using MediatR;

namespace AiCallCenter.Application.CallQueueItems.Queries.GetCallQueueItemById;

public sealed class GetCallQueueItemByIdQueryHandler
    : IRequestHandler<GetCallQueueItemByIdQuery, CallQueueItemListDto>
{
    private readonly ICallQueueItemRepository _repository;

    public GetCallQueueItemByIdQueryHandler(ICallQueueItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<CallQueueItemListDto> Handle(
        GetCallQueueItemByIdQuery request,
        CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(
            request.CompanyId,
            request.QueueItemId,
            cancellationToken);

        if (item is null)
        {
            throw new KeyNotFoundException("Kuyruk kaydı bulunamadı.");
        }

        return item.ToListDto();
    }
}
