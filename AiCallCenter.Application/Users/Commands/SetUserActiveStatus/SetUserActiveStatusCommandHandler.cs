using AiCallCenter.Application.Users;
using AiCallCenter.Application.Users.DTOs;
using AiCallCenter.Application.Users.Interfaces;
using MediatR;

namespace AiCallCenter.Application.Users.Commands.SetUserActiveStatus;

public sealed class SetUserActiveStatusCommandHandler
    : IRequestHandler<SetUserActiveStatusCommand, UserDto>
{
    private readonly IUserRepository _userRepository;

    public SetUserActiveStatusCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto> Handle(
        SetUserActiveStatusCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(
            request.CompanyId,
            request.UserId,
            cancellationToken);

        if (user is null)
            throw new KeyNotFoundException("Kullanıcı bulunamadı.");

        if (request.IsActive)
            user.Activate();
        else
            user.Deactivate();

        await _userRepository.SaveChangesAsync(cancellationToken);

        return user.ToDto();
    }
}
