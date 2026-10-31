using AiCallCenter.Application.CallQueues.Dtos;
using AiCallCenter.Application.CallQueues.Interfaces;
using MediatR;

namespace AiCallCenter.Application.CallQueues.Queries
    .GetCallQueues;

public sealed class GetCallQueuesQueryHandler
    : IRequestHandler<
        GetCallQueuesQuery,
        IReadOnlyList<CallQueueDto>>
{
    private readonly ICallQueueRepository _repository;

    public GetCallQueuesQueryHandler(
        ICallQueueRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<CallQueueDto>> Handle(
        GetCallQueuesQuery request,
        CancellationToken cancellationToken)
    {
        var queues = await _repository.GetAllAsync(
            request.CompanyId,
            cancellationToken);

        var result = new List<CallQueueDto>();

        foreach (var queue in queues)
        {
            var operatorCount =
                await _repository.GetOperatorCountAsync(
                    request.CompanyId,
                    queue.Id,
                    cancellationToken);

            var waitingCallCount =
                await _repository.GetWaitingCallCountAsync(
                    request.CompanyId,
                    queue.Id,
                    cancellationToken);

            result.Add(
                new CallQueueDto(
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
                    waitingCallCount));
        }

        return result;
    }
}