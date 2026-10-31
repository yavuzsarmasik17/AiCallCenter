using AiCallCenter.Domain.Entities;

namespace AiCallCenter.Application.Users.Interfaces;

public interface IUserRepository
{
    Task<bool> CompanyExistsAsync(
        Guid companyId,
        CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(
        Guid companyId,
        string email,
        CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(
        Guid companyId,
        string email,
        Guid? excludedUserId,
        CancellationToken cancellationToken = default);

    Task<User?> GetByIdAsync(
        Guid companyId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<User>> GetAllAsync(
        Guid companyId,
        CancellationToken cancellationToken = default);

    Task<int> CountActiveAdminsAsync(
        Guid companyId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        User user,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
