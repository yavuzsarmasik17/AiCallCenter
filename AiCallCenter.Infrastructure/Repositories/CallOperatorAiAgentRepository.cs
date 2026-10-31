using AiCallCenter.Application.CallOperatorAiAgents.Interfaces;
using AiCallCenter.Domain.Entities;
using AiCallCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AiCallCenter.Infrastructure.Repositories;

public sealed class CallOperatorAiAgentRepository
    : ICallOperatorAiAgentRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CallOperatorAiAgentRepository(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> CallOperatorExistsAsync(
        Guid companyId,
        Guid callOperatorId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.CallOperators
            .AnyAsync(
                x =>
                    x.Id == callOperatorId &&
                    x.CompanyId == companyId,
                cancellationToken);
    }

    public async Task<bool> AiAgentExistsAsync(
        Guid companyId,
        Guid aiAgentId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.AiAgents
            .AnyAsync(
                x =>
                    x.Id == aiAgentId &&
                    x.CompanyId == companyId &&
                    x.IsActive,
                cancellationToken);
    }

    public async Task<bool> AssignmentExistsAsync(
        Guid companyId,
        Guid callOperatorId,
        Guid aiAgentId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.CallOperatorAiAgents
            .AnyAsync(
                x =>
                    x.CompanyId == companyId &&
                    x.CallOperatorId == callOperatorId &&
                    x.AiAgentId == aiAgentId,
                cancellationToken);
    }

    public async Task<CallOperatorAiAgent?> GetByIdAsync(
        Guid companyId,
        Guid callOperatorId,
        Guid assignmentId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.CallOperatorAiAgents
            .Include(x => x.CallOperator)
            .Include(x => x.AiAgent)
            .FirstOrDefaultAsync(
                x =>
                    x.Id == assignmentId &&
                    x.CompanyId == companyId &&
                    x.CallOperatorId == callOperatorId,
                cancellationToken);
    }

    public async Task<CallOperatorAiAgent?> GetPrimaryAsync(
        Guid companyId,
        Guid callOperatorId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.CallOperatorAiAgents
            .FirstOrDefaultAsync(
                x =>
                    x.CompanyId == companyId &&
                    x.CallOperatorId == callOperatorId &&
                    x.IsPrimary &&
                    x.IsActive,
                cancellationToken);
    }

    public async Task<IReadOnlyList<CallOperatorAiAgent>>
        GetByOperatorAsync(
            Guid companyId,
            Guid callOperatorId,
            CancellationToken cancellationToken = default)
    {
        return await _dbContext.CallOperatorAiAgents
            .AsNoTracking()
            .Include(x => x.CallOperator)
            .Include(x => x.AiAgent)
            .Where(
                x =>
                    x.CompanyId == companyId &&
                    x.CallOperatorId == callOperatorId)
            .OrderByDescending(x => x.IsPrimary)
            .ThenByDescending(x => x.IsActive)
            .ThenBy(x => x.AiAgent.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AiAgent>>
        GetAvailableAiAgentsAsync(
            Guid companyId,
            CancellationToken cancellationToken = default)
    {
        return await _dbContext.AiAgents
            .AsNoTracking()
            .Where(
                x =>
                    x.CompanyId == companyId &&
                    x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        CallOperatorAiAgent assignment,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.CallOperatorAiAgents.AddAsync(
            assignment,
            cancellationToken);
    }

    public void Remove(CallOperatorAiAgent assignment)
    {
        _dbContext.CallOperatorAiAgents.Remove(assignment);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}