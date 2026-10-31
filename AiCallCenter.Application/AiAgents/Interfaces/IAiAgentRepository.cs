using AiCallCenter.Domain.Entities;

namespace AiCallCenter.Application.AiAgents.Interfaces;

public interface IAiAgentRepository
{
    Task<bool> CompanyExistsAsync(
        Guid companyId,
        CancellationToken cancellationToken = default);

    Task<bool> NameExistsAsync(
        Guid companyId,
        string name,
        CancellationToken cancellationToken = default);

    Task<bool> NameExistsForAnotherAgentAsync(
        Guid companyId,
        Guid aiAgentId,
        string name,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        AiAgent aiAgent,
        CancellationToken cancellationToken = default);

    Task<AiAgent?> GetByIdAsync(
        Guid companyId,
        Guid aiAgentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AiAgent>> GetByCompanyIdAsync(
        Guid companyId,
        CancellationToken cancellationToken = default);

    void Delete(AiAgent aiAgent);

    Task<bool> DeleteWithRelatedDataAsync(
        Guid companyId,
        Guid aiAgentId,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}