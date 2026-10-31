using AiCallCenter.Application.Users.Interfaces;
using AiCallCenter.Domain.Entities;
using AiCallCenter.Domain.Enums;
using AiCallCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AiCallCenter.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> CompanyExistsAsync(
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Companies.AnyAsync(
            company => company.Id == companyId && company.IsActive,
            cancellationToken);
    }

    public Task<bool> EmailExistsAsync(
        Guid companyId,
        string email,
        CancellationToken cancellationToken = default)
    {
        return EmailExistsAsync(
            companyId,
            email,
            null,
            cancellationToken);
    }

    public Task<bool> EmailExistsAsync(
        Guid companyId,
        string email,
        Guid? excludedUserId,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return _dbContext.Users.AnyAsync(
            user =>
                user.CompanyId == companyId &&
                !user.IsDeleted &&
                user.Email == normalizedEmail &&
                (!excludedUserId.HasValue || user.Id != excludedUserId.Value),
            cancellationToken);
    }

    public Task<User?> GetByIdAsync(
        Guid companyId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Users.FirstOrDefaultAsync(
            user =>
                user.CompanyId == companyId &&
                user.Id == userId &&
                !user.IsDeleted,
            cancellationToken);
    }

    public Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return _dbContext.Users.FirstOrDefaultAsync(
            user =>
                user.Email == normalizedEmail &&
                !user.IsDeleted,
            cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(user =>
                user.CompanyId == companyId &&
                !user.IsDeleted)
            .OrderByDescending(user => user.IsActive)
            .ThenBy(user => user.FirstName)
            .ThenBy(user => user.LastName)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountActiveAdminsAsync(
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Users.CountAsync(
            user =>
                user.CompanyId == companyId &&
                !user.IsDeleted &&
                user.IsActive &&
                (user.Role == UserRole.CompanyAdmin ||
                 user.Role == UserRole.SuperAdmin),
            cancellationToken);
    }

    public async Task AddAsync(
        User user,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Users.AddAsync(user, cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
