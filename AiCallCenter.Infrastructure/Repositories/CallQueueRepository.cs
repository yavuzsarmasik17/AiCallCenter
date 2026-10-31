using AiCallCenter.Application.CallQueues.Interfaces;
using AiCallCenter.Domain.Entities;
using AiCallCenter.Domain.Enums;
using AiCallCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AiCallCenter.Infrastructure.Repositories;

public sealed class CallQueueRepository
    : ICallQueueRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CallQueueRepository(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CallQueue>> GetAllAsync(
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.CallQueues
            .AsNoTracking()
            .Include(x => x.AiAgent)
            .Where(x => x.CompanyId == companyId)
            .OrderByDescending(x => x.IsActive)
            .ThenByDescending(x => x.Priority)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<CallQueue?> GetByIdAsync(
        Guid companyId,
        Guid callQueueId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.CallQueues
            .Include(x => x.AiAgent)
            .FirstOrDefaultAsync(
                x =>
                    x.CompanyId == companyId &&
                    x.Id == callQueueId,
                cancellationToken);
    }

    public Task<bool> NameExistsAsync(
        Guid companyId,
        string name,
        Guid? excludedQueueId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim();

        return _dbContext.CallQueues
            .AnyAsync(
                x =>
                    x.CompanyId == companyId &&
                    x.Name == normalizedName &&
                    (!excludedQueueId.HasValue ||
                     x.Id != excludedQueueId.Value),
                cancellationToken);
    }

    public Task<bool> AiAgentExistsAsync(
        Guid companyId,
        Guid aiAgentId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.AiAgents
            .AnyAsync(
                x =>
                    x.CompanyId == companyId &&
                    x.Id == aiAgentId &&
                    x.IsActive,
                cancellationToken);
    }

    public Task<int> GetWaitingCallCountAsync(
        Guid companyId,
        Guid callQueueId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.CallQueueItems
            .CountAsync(
                x =>
                    x.CompanyId == companyId &&
                    x.CallQueueId == callQueueId &&
                    x.Status == CallQueueStatus.Waiting,
                cancellationToken);
    }

    public Task<int> GetOperatorCountAsync(
        Guid companyId,
        Guid callQueueId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.CallQueueOperators
            .CountAsync(
                x =>
                    x.CompanyId == companyId &&
                    x.CallQueueId == callQueueId &&
                    x.IsActive,
                cancellationToken);
    }

    public async Task AddAsync(
        CallQueue callQueue,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.CallQueues.AddAsync(
            callQueue,
            cancellationToken);
    }

    public void Remove(CallQueue callQueue)
    {
        _dbContext.CallQueues.Remove(callQueue);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}