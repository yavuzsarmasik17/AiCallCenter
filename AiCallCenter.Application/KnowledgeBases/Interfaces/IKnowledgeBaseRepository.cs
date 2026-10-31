using AiCallCenter.Domain.Entities;

namespace AiCallCenter.Application.KnowledgeBases.Interfaces;

public interface IKnowledgeBaseRepository
{
    Task<bool> CompanyExistsAsync(
        Guid companyId,
        CancellationToken cancellationToken = default);

    Task<bool> NameExistsAsync(
        Guid companyId,
        string name,
        CancellationToken cancellationToken = default);

    Task<bool> NameExistsForAnotherKnowledgeBaseAsync(
        Guid companyId,
        Guid knowledgeBaseId,
        string name,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        KnowledgeBase knowledgeBase,
        CancellationToken cancellationToken = default);

    Task<KnowledgeBase?> GetByIdAsync(
        Guid companyId,
        Guid knowledgeBaseId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<KnowledgeBase>> GetByCompanyIdAsync(
        Guid companyId,
        CancellationToken cancellationToken = default);

    void Delete(
        KnowledgeBase knowledgeBase);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}