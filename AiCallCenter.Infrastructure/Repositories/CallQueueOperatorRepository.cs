using AiCallCenter.Application.CallQueues.Interfaces;
using AiCallCenter.Domain.Entities;
using AiCallCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AiCallCenter.Infrastructure.Repositories;

public sealed class CallQueueOperatorRepository
    : ICallQueueOperatorRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CallQueueOperatorRepository(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> OperatorExistsAsync(
        Guid companyId,
        Guid callOperatorId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.CallOperators
            .AnyAsync(
                x =>
                    x.CompanyId == companyId &&
                    x.Id == callOperatorId,
                cancellationToken);
    }

    public Task<bool> AssignmentExistsAsync(
        Guid companyId,
        Guid callQueueId,
        Guid callOperatorId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.CallQueueOperators
            .AnyAsync(
                x =>
                    x.CompanyId == companyId &&
                    x.CallQueueId == callQueueId &&
                    x.CallOperatorId == callOperatorId,
                cancellationToken);
    }

    public Task<CallQueueOperator?> GetByIdAsync(
        Guid companyId,
        Guid callQueueId,
        Guid assignmentId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.CallQueueOperators
            .Include(x => x.CallOperator)
            .FirstOrDefaultAsync(
                x =>
                    x.CompanyId == companyId &&
                    x.CallQueueId == callQueueId &&
                    x.Id == assignmentId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<CallQueueOperator>>
        GetAllAsync(
            Guid companyId,
            Guid callQueueId,
            CancellationToken cancellationToken = default)
    {
        return await _dbContext.CallQueueOperators
            .AsNoTracking()
            .Include(x => x.CallOperator)
            .Where(
                x =>
                    x.CompanyId == companyId &&
                    x.CallQueueId == callQueueId)
            .OrderByDescending(x => x.IsActive)
            .ThenByDescending(x => x.Priority)
            .ThenBy(x => x.CallOperator.DisplayName)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        CallQueueOperator assignment,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.CallQueueOperators.AddAsync(
            assignment,
            cancellationToken);
    }

    public void Remove(CallQueueOperator assignment)
    {
        _dbContext.CallQueueOperators.Remove(assignment);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}