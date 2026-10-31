using AiCallCenter.Application.Calls.Interfaces;
using AiCallCenter.Domain.Entities;
using AiCallCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AiCallCenter.Infrastructure.Persistence.Repositories;

public sealed class CallMessageRepository : ICallMessageRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CallMessageRepository(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CallMessage?> GetByIdAsync(
        Guid companyId,
        Guid callId,
        Guid messageId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.CallMessages
            .AsNoTracking()
            .Where(x =>
                x.Id == messageId &&
                x.CallId == callId &&
                x.Call.CompanyId == companyId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CallMessage>> GetByCallIdAsync(
        Guid companyId,
        Guid callId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.CallMessages
            .AsNoTracking()
            .Where(x =>
                x.CallId == callId &&
                x.Call.CompanyId == companyId)
            .OrderBy(x => x.SequenceNumber)
            .ThenBy(x => x.SpokenAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetNextSequenceNumberAsync(
        Guid companyId,
        Guid callId,
        CancellationToken cancellationToken = default)
    {
        var lastSequenceNumber =
            await _dbContext.CallMessages
                .Where(x =>
                    x.CallId == callId &&
                    x.Call.CompanyId == companyId)
                .Select(x => (int?)x.SequenceNumber)
                .MaxAsync(cancellationToken);

        return (lastSequenceNumber ?? 0) + 1;
    }

    public async Task AddAsync(
        CallMessage callMessage,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.CallMessages.AddAsync(
            callMessage,
            cancellationToken);
    }

    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}