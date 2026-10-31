using AiCallCenter.Application.Users;
using AiCallCenter.Application.Users.DTOs;
using AiCallCenter.Application.Users.Interfaces;
using MediatR;

namespace AiCallCenter.Application.Users.Commands.UpdateUser;

public sealed class UpdateUserCommandHandler
    : IRequestHandler<UpdateUserCommand, UserDto>
{
    private readonly IUserRepository _userRepository;

    public UpdateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto> Handle(
        UpdateUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(
            request.CompanyId,
            request.UserId,
            cancellationToken);

        if (user is null)
            throw new KeyNotFoundException("Kullanıcı bulunamadı.");

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        if (await _userRepository.EmailExistsAsync(
            request.CompanyId,
            normalizedEmail,
            request.UserId,
            cancellationToken))
        {
            throw new InvalidOperationException(
                "Bu e-posta adresi başka bir kullanıcı tarafından kullanılıyor.");
        }

        user.UpdateProfile(
            request.FirstName,
            request.LastName,
            normalizedEmail);

        await _userRepository.SaveChangesAsync(cancellationToken);

        return user.ToDto();
    }
}
