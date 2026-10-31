using AiCallCenter.Domain.Entities;

namespace AiCallCenter.Application.Companies.Interfaces;

public interface ICompanyRepository
{
    Task<bool> SlugExistsAsync(
        string slug,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Company company,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Company>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}