using AiCallCenter.Domain.Entities;

namespace AiCallCenter.Application.Calls.Interfaces;

public interface ICallMessageRepository
{
    Task<CallMessage?> GetByIdAsync(
        Guid companyId,
        Guid callId,
        Guid messageId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CallMessage>> GetByCallIdAsync(
        Guid companyId,
        Guid callId,
        CancellationToken cancellationToken = default);

    Task<int> GetNextSequenceNumberAsync(
        Guid companyId,
        Guid callId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        CallMessage callMessage,
        CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}