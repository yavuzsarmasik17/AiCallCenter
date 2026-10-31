using AiCallCenter.Application.KnowledgeBases.Interfaces;
using AiCallCenter.Domain.Entities;
using AiCallCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AiCallCenter.Infrastructure.Repositories;

public sealed class KnowledgeBaseRepository
    : IKnowledgeBaseRepository
{
    private readonly ApplicationDbContext _dbContext;

    public KnowledgeBaseRepository(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> CompanyExistsAsync(
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Companies.AnyAsync(
            company => company.Id == companyId,
            cancellationToken);
    }

    public Task<bool> NameExistsAsync(
        Guid companyId,
        string name,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.KnowledgeBases.AnyAsync(
            knowledgeBase =>
                knowledgeBase.CompanyId == companyId &&
                knowledgeBase.Name == name,
            cancellationToken);
    }

    public Task<bool> NameExistsForAnotherKnowledgeBaseAsync(
        Guid companyId,
        Guid knowledgeBaseId,
        string name,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.KnowledgeBases.AnyAsync(
            knowledgeBase =>
                knowledgeBase.CompanyId == companyId &&
                knowledgeBase.Id != knowledgeBaseId &&
                knowledgeBase.Name == name,
            cancellationToken);
    }

    public async Task AddAsync(
        KnowledgeBase knowledgeBase,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.KnowledgeBases.AddAsync(
            knowledgeBase,
            cancellationToken);
    }

    public Task<KnowledgeBase?> GetByIdAsync(
        Guid companyId,
        Guid knowledgeBaseId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.KnowledgeBases
            .FirstOrDefaultAsync(
                knowledgeBase =>
                    knowledgeBase.Id == knowledgeBaseId &&
                    knowledgeBase.CompanyId == companyId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<KnowledgeBase>>
        GetByCompanyIdAsync(
            Guid companyId,
            CancellationToken cancellationToken = default)
    {
        return await _dbContext.KnowledgeBases
            .AsNoTracking()
            .Where(knowledgeBase =>
                knowledgeBase.CompanyId == companyId)
            .OrderByDescending(knowledgeBase =>
                knowledgeBase.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public void Delete(
        KnowledgeBase knowledgeBase)
    {
        _dbContext.KnowledgeBases.Remove(
            knowledgeBase);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}