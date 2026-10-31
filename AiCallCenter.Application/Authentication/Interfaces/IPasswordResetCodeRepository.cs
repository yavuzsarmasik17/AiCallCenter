using AiCallCenter.Domain.Entities;

namespace AiCallCenter.Application.Authentication.Interfaces;

public interface IPasswordResetCodeRepository
{
    Task<PasswordResetCode?> GetLatestActiveByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PasswordResetCode>> GetActiveCodesByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        PasswordResetCode passwordResetCode,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}