using AiCallCenter.Domain.Entities;

namespace AiCallCenter.Application.Calls.Interfaces;

public interface ICallRepository
{
    Task<bool> CompanyExistsAsync(
        Guid companyId,
        CancellationToken cancellationToken = default);

    Task<bool> AiAgentExistsAsync(
        Guid companyId,
        Guid aiAgentId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Call call,
        CancellationToken cancellationToken = default);

    Task<Call?> GetByIdAsync(
        Guid companyId,
        Guid callId,
        CancellationToken cancellationToken = default);

    // Call üzerinde durum değişikliği yapılacağı zaman kullanılır.
    // EF Core entity'yi takip eder.
    Task<Call?> GetTrackedByIdAsync(
        Guid companyId,
        Guid callId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Call>> GetAllAsync(
        Guid companyId,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}