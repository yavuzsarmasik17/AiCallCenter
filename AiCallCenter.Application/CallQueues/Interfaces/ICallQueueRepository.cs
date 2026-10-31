using AiCallCenter.Domain.Entities;

namespace AiCallCenter.Application.CallQueues.Interfaces;

public interface ICallQueueRepository
{
    Task<IReadOnlyList<CallQueue>> GetAllAsync(
        Guid companyId,
        CancellationToken cancellationToken = default);

    Task<CallQueue?> GetByIdAsync(
        Guid companyId,
        Guid callQueueId,
        CancellationToken cancellationToken = default);

    Task<bool> NameExistsAsync(
        Guid companyId,
        string name,
        Guid? excludedQueueId = null,
        CancellationToken cancellationToken = default);

    Task<bool> AiAgentExistsAsync(
        Guid companyId,
        Guid aiAgentId,
        CancellationToken cancellationToken = default);

    Task<int> GetWaitingCallCountAsync(
        Guid companyId,
        Guid callQueueId,
        CancellationToken cancellationToken = default);

    Task<int> GetOperatorCountAsync(
        Guid companyId,
        Guid callQueueId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        CallQueue callQueue,
        CancellationToken cancellationToken = default);

    void Remove(CallQueue callQueue);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}