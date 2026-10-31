using AiCallCenter.Application.Companies.Interfaces;
using AiCallCenter.Domain.Entities;
using AiCallCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AiCallCenter.Infrastructure.Repositories;

public sealed class CompanyRepository : ICompanyRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CompanyRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> SlugExistsAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Companies
            .AnyAsync(
                x => x.Slug == slug,
                cancellationToken);
    }

    public async Task AddAsync(
        Company company,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Companies.AddAsync(
            company,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Company>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Companies
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}