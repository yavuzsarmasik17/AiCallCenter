using AiCallCenter.Application.Users;
using AiCallCenter.Application.Users.DTOs;
using AiCallCenter.Application.Users.Interfaces;
using MediatR;

namespace AiCallCenter.Application.Users.Commands.ChangeUserRole;

public sealed class ChangeUserRoleCommandHandler
    : IRequestHandler<ChangeUserRoleCommand, UserDto>
{
    private readonly IUserRepository _userRepository;

    public ChangeUserRoleCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto> Handle(
        ChangeUserRoleCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(
            request.CompanyId,
            request.UserId,
            cancellationToken);

        if (user is null)
            throw new KeyNotFoundException("Kullanıcı bulunamadı.");

        user.ChangeRole(request.Role);

        await _userRepository.SaveChangesAsync(cancellationToken);

        return user.ToDto();
    }
}
