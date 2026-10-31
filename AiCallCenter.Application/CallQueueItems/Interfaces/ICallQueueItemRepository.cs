using AiCallCenter.Domain.Entities;
using AiCallCenter.Domain.Enums;

namespace AiCallCenter.Application.CallQueueItems.Interfaces;

public interface ICallQueueItemRepository
{
    Task<CallQueue?> GetQueueAsync(Guid companyId, Guid callQueueId, CancellationToken cancellationToken = default);
    Task<Call?> GetCallAsync(Guid companyId, Guid callId, CancellationToken cancellationToken = default);
    Task<CallQueueItem?> GetByIdAsync(Guid companyId, Guid queueItemId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CallQueueItem>> GetAllAsync(
        Guid companyId,
        CallQueueStatus? status = null,
        Guid? callQueueId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CallQueueItem>> GetWaitingAsync(
        Guid companyId,
        Guid? callQueueId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CallQueueItem>> GetByOperatorAsync(
        Guid companyId,
        Guid operatorId,
        bool activeOnly = true,
        CancellationToken cancellationToken = default);

    Task<bool> ActiveItemExistsAsync(Guid companyId, Guid callId, CancellationToken cancellationToken = default);
    Task<int> GetActiveQueueSizeAsync(Guid companyId, Guid callQueueId, CancellationToken cancellationToken = default);
    Task AddAsync(CallQueueItem queueItem, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
