using AiCallCenter.Application.AiAgents.Interfaces;
using AiCallCenter.Domain.Entities;
using AiCallCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AiCallCenter.Infrastructure.Repositories;

public sealed class AiAgentKnowledgeBaseRepository
    : IAiAgentKnowledgeBaseRepository
{
    private readonly ApplicationDbContext _dbContext;

    public AiAgentKnowledgeBaseRepository(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> AiAgentExistsAsync(
        Guid companyId,
        Guid aiAgentId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.AiAgents.AnyAsync(
            agent =>
                agent.Id == aiAgentId &&
                agent.CompanyId == companyId,
            cancellationToken);
    }

    public Task<bool> KnowledgeBaseExistsAsync(
        Guid companyId,
        Guid knowledgeBaseId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.KnowledgeBases.AnyAsync(
            knowledgeBase =>
                knowledgeBase.Id == knowledgeBaseId &&
                knowledgeBase.CompanyId == companyId &&
                knowledgeBase.IsActive,
            cancellationToken);
    }

    public Task<bool> ConnectionExistsAsync(
        Guid aiAgentId,
        Guid knowledgeBaseId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.AiAgentKnowledgeBases.AnyAsync(
            connection =>
                connection.AiAgentId == aiAgentId &&
                connection.KnowledgeBaseId == knowledgeBaseId,
            cancellationToken);
    }

    public Task<AiAgentKnowledgeBase?> GetConnectionAsync(
        Guid aiAgentId,
        Guid knowledgeBaseId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.AiAgentKnowledgeBases
            .FirstOrDefaultAsync(
                connection =>
                    connection.AiAgentId == aiAgentId &&
                    connection.KnowledgeBaseId == knowledgeBaseId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<AiAgentKnowledgeBase>>
    GetByAiAgentIdAsync(
        Guid companyId,
        Guid aiAgentId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.AiAgentKnowledgeBases
            .AsNoTracking()
            .Include(connection => connection.KnowledgeBase)
            .Where(connection =>
                connection.AiAgentId == aiAgentId &&
                connection.AiAgent.CompanyId == companyId)
            .OrderBy(connection => connection.KnowledgeBase.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        AiAgentKnowledgeBase connection,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.AiAgentKnowledgeBases.AddAsync(
            connection,
            cancellationToken);
    }

    public void Remove(AiAgentKnowledgeBase connection)
    {
        _dbContext.AiAgentKnowledgeBases.Remove(connection);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}