using AiCallCenter.Application.CallRouting.Interfaces;
using AiCallCenter.Domain.Entities;
using AiCallCenter.Domain.Enums;
using AiCallCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AiCallCenter.Infrastructure.Repositories;

public sealed class CallRoutingRepository
    : ICallRoutingRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CallRoutingRepository(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<CallQueueItem?> GetQueueItemAsync(
        Guid companyId,
        Guid queueItemId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.CallQueueItems
            .Include(x => x.CallQueue)
            .Include(x => x.Call)
            .FirstOrDefaultAsync(
                x =>
                    x.CompanyId == companyId &&
                    x.Id == queueItemId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<
        RoutingOperatorCandidate>>
        GetAvailableOperatorsAsync(
            Guid companyId,
            Guid callQueueId,
            CancellationToken cancellationToken = default)
    {
        return await (
            from assignment
                in _dbContext.CallQueueOperators
            join callOperator
                in _dbContext.CallOperators
                on assignment.CallOperatorId
                equals callOperator.Id
            where
                assignment.CompanyId == companyId &&
                assignment.CallQueueId == callQueueId &&
                assignment.IsActive &&
                callOperator.CompanyId == companyId &&
                callOperator.CanReceiveCalls &&
                callOperator.Status ==
                    CallOperatorStatus.Available &&
                callOperator.ActiveCallCount <
                    callOperator.MaxConcurrentCalls &&
                callOperator.ExtensionNumber != null &&
                callOperator.ExtensionNumber != ""
            select new RoutingOperatorCandidate(
                callOperator,
                assignment.Priority)
        ).ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}
