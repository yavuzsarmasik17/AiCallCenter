using AiCallCenter.Application.Users.Interfaces;
using AiCallCenter.Domain.Enums;
using MediatR;

namespace AiCallCenter.Application.Users.Commands.DeleteUser;

public sealed class DeleteUserCommandHandler
    : IRequestHandler<DeleteUserCommand>
{
    private readonly IUserRepository _userRepository;

    public DeleteUserCommandHandler(
        IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task Handle(
        DeleteUserCommand request,
        CancellationToken cancellationToken)
    {
        if (request.UserId == request.DeletedByUserId)
        {
            throw new InvalidOperationException(
                "Kendi kullanıcı hesabınızı silemezsiniz.");
        }

        var user = await _userRepository.GetByIdAsync(
            request.CompanyId,
            request.UserId,
            cancellationToken);

        if (user is null)
        {
            throw new KeyNotFoundException(
                "Silinecek kullanıcı bulunamadı.");
        }

        var isAdmin =
            user.Role == UserRole.CompanyAdmin ||
            user.Role == UserRole.SuperAdmin;

        if (isAdmin && user.IsActive)
        {
            var activeAdminCount =
                await _userRepository.CountActiveAdminsAsync(
                    request.CompanyId,
                    cancellationToken);

            if (activeAdminCount <= 1)
            {
                throw new InvalidOperationException(
                    "Firmanın son aktif yöneticisi silinemez.");
            }
        }

        user.Delete(request.DeletedByUserId);

        await _userRepository.SaveChangesAsync(
            cancellationToken);
    }
}
