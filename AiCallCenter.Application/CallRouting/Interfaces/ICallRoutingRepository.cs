using AiCallCenter.Domain.Entities;

namespace AiCallCenter.Application.CallRouting.Interfaces;

public interface ICallRoutingRepository
{
    Task<CallQueueItem?> GetQueueItemAsync(
        Guid companyId,
        Guid queueItemId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RoutingOperatorCandidate>>
        GetAvailableOperatorsAsync(
            Guid companyId,
            Guid callQueueId,
            CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}

public sealed record RoutingOperatorCandidate(
    CallOperator Operator,
    int QueuePriority);
