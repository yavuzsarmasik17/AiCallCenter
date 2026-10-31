using AiCallCenter.Application.Authentication.Interfaces;
using AiCallCenter.Domain.Entities;
using AiCallCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AiCallCenter.Infrastructure.Repositories;

public sealed class PasswordResetCodeRepository
    : IPasswordResetCodeRepository
{
    private readonly ApplicationDbContext _dbContext;

    public PasswordResetCodeRepository(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<PasswordResetCode?> GetLatestActiveByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return _dbContext.PasswordResetCodes
            .Where(code =>
                code.UserId == userId &&
                !code.IsUsed &&
                code.ExpiresAt > now &&
                code.FailedAttemptCount < 5)
            .OrderByDescending(code => code.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PasswordResetCode>>
        GetActiveCodesByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await _dbContext.PasswordResetCodes
            .Where(code =>
                code.UserId == userId &&
                !code.IsUsed &&
                code.ExpiresAt > now)
            .OrderByDescending(code => code.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        PasswordResetCode passwordResetCode,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.PasswordResetCodes.AddAsync(
            passwordResetCode,
            cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}