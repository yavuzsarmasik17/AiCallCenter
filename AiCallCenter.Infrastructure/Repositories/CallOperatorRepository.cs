using AiCallCenter.Application.CallOperators.Interfaces;
using AiCallCenter.Domain.Entities;
using AiCallCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AiCallCenter.Infrastructure.Repositories;

public sealed class CallOperatorRepository : ICallOperatorRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CallOperatorRepository(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CallOperator>>
        GetAllByCompanyAsync(
            Guid companyId,
            CancellationToken cancellationToken = default)
    {
        return await _dbContext.CallOperators
            .AsNoTracking()
            .Where(x => x.CompanyId == companyId)
            .OrderBy(x => x.DisplayName)
            .ToListAsync(cancellationToken);
    }

    public Task<CallOperator?> GetByIdAsync(
        Guid companyId,
        Guid operatorId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.CallOperators
            .FirstOrDefaultAsync(
                x =>
                    x.CompanyId == companyId &&
                    x.Id == operatorId,
                cancellationToken);
    }

    public Task<CallOperator?> GetByUserIdAsync(
        Guid companyId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.CallOperators
            .FirstOrDefaultAsync(
                x =>
                    x.CompanyId == companyId &&
                    x.UserId == userId,
                cancellationToken);
    }

    public Task<bool> UserAlreadyOperatorAsync(
        Guid companyId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.CallOperators
            .AnyAsync(
                x =>
                    x.CompanyId == companyId &&
                    x.UserId == userId,
                cancellationToken);
    }

    public Task<bool> ExtensionNumberExistsAsync(
        Guid companyId,
        string extensionNumber,
        Guid? excludedOperatorId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedExtension = extensionNumber.Trim();

        return _dbContext.CallOperators
            .AnyAsync(
                x =>
                    x.CompanyId == companyId &&
                    x.ExtensionNumber == normalizedExtension &&
                    (!excludedOperatorId.HasValue ||
                     x.Id != excludedOperatorId.Value),
                cancellationToken);
    }

    public async Task AddAsync(
        CallOperator callOperator,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.CallOperators.AddAsync(
            callOperator,
            cancellationToken);
    }

    public void Remove(CallOperator callOperator)
    {
        _dbContext.CallOperators.Remove(callOperator);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}