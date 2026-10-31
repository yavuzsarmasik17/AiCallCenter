using AiCallCenter.Application.Calls.Interfaces;
using AiCallCenter.Domain.Entities;
using AiCallCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AiCallCenter.Infrastructure.Repositories;

public sealed class CallRepository : ICallRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CallRepository(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> CompanyExistsAsync(
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Companies.AnyAsync(
            x => x.Id == companyId,
            cancellationToken);
    }

    public Task<bool> AiAgentExistsAsync(
        Guid companyId,
        Guid aiAgentId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.AiAgents.AnyAsync(
            x =>
                x.Id == aiAgentId &&
                x.CompanyId == companyId &&
                x.IsActive,
            cancellationToken);
    }

    public async Task AddAsync(
        Call call,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Calls.AddAsync(
            call,
            cancellationToken);
    }

    // Salt okunur işlemler için.
    public Task<Call?> GetByIdAsync(
        Guid companyId,
        Guid callId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Calls
            .AsNoTracking()
            .Include(x => x.AiAgent)
            .FirstOrDefaultAsync(
                x =>
                    x.Id == callId &&
                    x.CompanyId == companyId,
                cancellationToken);
    }

    // Call üzerinde değişiklik yapılacak işlemler için.
    // AsNoTracking kullanılmaz.
    public Task<Call?> GetTrackedByIdAsync(
        Guid companyId,
        Guid callId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Calls
            .Include(x => x.AiAgent)
            .FirstOrDefaultAsync(
                x =>
                    x.Id == callId &&
                    x.CompanyId == companyId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Call>> GetAllAsync(
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Calls
            .AsNoTracking()
            .Include(x => x.AiAgent)
            .Where(x => x.CompanyId == companyId)
            .OrderByDescending(x => x.StartedAt)
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}