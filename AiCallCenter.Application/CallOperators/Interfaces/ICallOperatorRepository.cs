using AiCallCenter.Domain.Entities;

namespace AiCallCenter.Application.CallOperators.Interfaces;

public interface ICallOperatorRepository
{
    Task<IReadOnlyList<CallOperator>> GetAllByCompanyAsync(
        Guid companyId,
        CancellationToken cancellationToken = default);

    Task<CallOperator?> GetByIdAsync(
        Guid companyId,
        Guid operatorId,
        CancellationToken cancellationToken = default);

    Task<CallOperator?> GetByUserIdAsync(
        Guid companyId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> UserAlreadyOperatorAsync(
        Guid companyId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> ExtensionNumberExistsAsync(
        Guid companyId,
        string extensionNumber,
        Guid? excludedOperatorId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        CallOperator callOperator,
        CancellationToken cancellationToken = default);

    void Remove(CallOperator callOperator);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}