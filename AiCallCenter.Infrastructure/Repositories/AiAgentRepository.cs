using AiCallCenter.Application.AiAgents.Interfaces;
using AiCallCenter.Domain.Entities;
using AiCallCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AiCallCenter.Infrastructure.Repositories;

public sealed class AiAgentRepository
    : IAiAgentRepository
{
    private readonly ApplicationDbContext _dbContext;

    public AiAgentRepository(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> CompanyExistsAsync(
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Companies.AnyAsync(
            company => company.Id == companyId,
            cancellationToken);
    }

    public Task<bool> NameExistsAsync(
        Guid companyId,
        string name,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.AiAgents.AnyAsync(
            agent =>
                agent.CompanyId == companyId &&
                agent.Name == name,
            cancellationToken);
    }

    public Task<bool> NameExistsForAnotherAgentAsync(
        Guid companyId,
        Guid aiAgentId,
        string name,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.AiAgents.AnyAsync(
            agent =>
                agent.CompanyId == companyId &&
                agent.Id != aiAgentId &&
                agent.Name == name,
            cancellationToken);
    }

    public async Task AddAsync(
        AiAgent aiAgent,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.AiAgents.AddAsync(
            aiAgent,
            cancellationToken);
    }

    public Task<AiAgent?> GetByIdAsync(
        Guid companyId,
        Guid aiAgentId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.AiAgents
            .FirstOrDefaultAsync(
                agent =>
                    agent.Id == aiAgentId &&
                    agent.CompanyId == companyId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<AiAgent>>
        GetByCompanyIdAsync(
            Guid companyId,
            CancellationToken cancellationToken = default)
    {
        return await _dbContext.AiAgents
            .AsNoTracking()
            .Where(agent =>
                agent.CompanyId == companyId)
            .OrderByDescending(agent =>
                agent.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public void Delete(AiAgent aiAgent)
    {
        _dbContext.AiAgents.Remove(aiAgent);
    }

    public async Task<bool> DeleteWithRelatedDataAsync(
        Guid companyId,
        Guid aiAgentId,
        CancellationToken cancellationToken = default)
    {
        var agentExists =
            await _dbContext.AiAgents.AnyAsync(
                agent =>
                    agent.Id == aiAgentId &&
                    agent.CompanyId == companyId,
                cancellationToken);

        if (!agentExists)
        {
            return false;
        }

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            var callIds = _dbContext.Calls
                .Where(call =>
                    call.AiAgentId == aiAgentId &&
                    call.CompanyId == companyId)
                .Select(call => call.Id);

            await _dbContext.CallMessages
                .Where(message =>
                    callIds.Contains(message.CallId))
                .ExecuteDeleteAsync(
                    cancellationToken);

            await _dbContext.Calls
                .Where(call =>
                    call.AiAgentId == aiAgentId &&
                    call.CompanyId == companyId)
                .ExecuteDeleteAsync(
                    cancellationToken);

            var conversationIds =
                _dbContext.AiConversations
                    .Where(conversation =>
                        conversation.AiAgentId ==
                            aiAgentId &&
                        conversation.CompanyId ==
                            companyId)
                    .Select(conversation =>
                        conversation.Id);

            await _dbContext.AiConversationMessages
                .Where(message =>
                    conversationIds.Contains(
                        message.ConversationId))
                .ExecuteDeleteAsync(
                    cancellationToken);

            await _dbContext.AiConversations
                .Where(conversation =>
                    conversation.AiAgentId ==
                        aiAgentId &&
                    conversation.CompanyId ==
                        companyId)
                .ExecuteDeleteAsync(
                    cancellationToken);

            await _dbContext.AiAgentKnowledgeBases
                .Where(connection =>
                    connection.AiAgentId ==
                        aiAgentId)
                .ExecuteDeleteAsync(
                    cancellationToken);

            var deletedAgentCount =
                await _dbContext.AiAgents
                    .Where(agent =>
                        agent.Id == aiAgentId &&
                        agent.CompanyId == companyId)
                    .ExecuteDeleteAsync(
                        cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);

            return deletedAgentCount > 0;
        }
        catch
        {
            await transaction.RollbackAsync(
                cancellationToken);

            throw;
        }
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}