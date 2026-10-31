using AiCallCenter.Domain.Entities;

namespace AiCallCenter.Application.AiAgents.Interfaces;

public interface IAiAgentKnowledgeBaseRepository
{
    Task<bool> AiAgentExistsAsync(
        Guid companyId,
        Guid aiAgentId,
        CancellationToken cancellationToken = default);

    Task<bool> KnowledgeBaseExistsAsync(
        Guid companyId,
        Guid knowledgeBaseId,
        CancellationToken cancellationToken = default);

    Task<bool> ConnectionExistsAsync(
        Guid aiAgentId,
        Guid knowledgeBaseId,
        CancellationToken cancellationToken = default);

    Task<AiAgentKnowledgeBase?> GetConnectionAsync(
        Guid aiAgentId,
        Guid knowledgeBaseId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AiAgentKnowledgeBase>> GetByAiAgentIdAsync(
        Guid companyId,
        Guid aiAgentId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        AiAgentKnowledgeBase connection,
        CancellationToken cancellationToken = default);

    void Remove(AiAgentKnowledgeBase connection);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}