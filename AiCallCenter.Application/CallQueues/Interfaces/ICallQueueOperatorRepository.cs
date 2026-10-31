using AiCallCenter.Domain.Entities;

namespace AiCallCenter.Application.CallQueues.Interfaces;

public interface ICallQueueOperatorRepository
{
    Task<bool> OperatorExistsAsync(
        Guid companyId,
        Guid callOperatorId,
        CancellationToken cancellationToken = default);

    Task<bool> AssignmentExistsAsync(
        Guid companyId,
        Guid callQueueId,
        Guid callOperatorId,
        CancellationToken cancellationToken = default);

    Task<CallQueueOperator?> GetByIdAsync(
        Guid companyId,
        Guid callQueueId,
        Guid assignmentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CallQueueOperator>> GetAllAsync(
        Guid companyId,
        Guid callQueueId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        CallQueueOperator assignment,
        CancellationToken cancellationToken = default);

    void Remove(CallQueueOperator assignment);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}