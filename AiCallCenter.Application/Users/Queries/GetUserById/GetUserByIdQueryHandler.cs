using AiCallCenter.Application.Users;
using AiCallCenter.Application.Users.DTOs;
using AiCallCenter.Application.Users.Interfaces;
using MediatR;

namespace AiCallCenter.Application.Users.Queries.GetUserById;

public sealed class GetUserByIdQueryHandler
    : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto> Handle(
        GetUserByIdQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(
            request.CompanyId,
            request.UserId,
            cancellationToken);

        if (user is null)
            throw new KeyNotFoundException("Kullanıcı bulunamadı.");

        return user.ToDto();
    }
}
