using AiCallCenter.Application.CallQueues.Dtos;
using AiCallCenter.Application.CallQueues.Interfaces;
using MediatR;

namespace AiCallCenter.Application.CallQueues.Queries
    .GetCallQueueById;

public sealed class GetCallQueueByIdQueryHandler
    : IRequestHandler<
        GetCallQueueByIdQuery,
        CallQueueDto>
{
    private readonly ICallQueueRepository _repository;

    public GetCallQueueByIdQueryHandler(
        ICallQueueRepository repository)
    {
        _repository = repository;
    }

    public async Task<CallQueueDto> Handle(
        GetCallQueueByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (request.CompanyId == Guid.Empty)
        {
            throw new ArgumentException(
                "Firma kimliği geçersiz.");
        }

        if (request.CallQueueId == Guid.Empty)
        {
            throw new ArgumentException(
                "Kuyruk kimliği geçersiz.");
        }

        var queue = await _repository.GetByIdAsync(
            request.CompanyId,
            request.CallQueueId,
            cancellationToken);

        if (queue is null)
        {
            throw new KeyNotFoundException(
                "Çağrı kuyruğu bulunamadı.");
        }

        var operatorCount =
            await _repository.GetOperatorCountAsync(
                request.CompanyId,
                request.CallQueueId,
                cancellationToken);

        var waitingCallCount =
            await _repository.GetWaitingCallCountAsync(
                request.CompanyId,
                request.CallQueueId,
                cancellationToken);

        return new CallQueueDto(
            queue.Id,
            queue.CompanyId,
            queue.AiAgentId,
            queue.AiAgent?.Name,
            queue.Name,
            queue.Description,
            queue.Priority,
            queue.MaxWaitingSeconds,
            queue.MaxQueueSize,
            queue.IsActive,
            operatorCount,
            waitingCallCount);
    }
}