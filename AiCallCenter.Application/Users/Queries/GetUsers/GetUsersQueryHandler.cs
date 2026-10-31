using AiCallCenter.Application.Users;
using AiCallCenter.Application.Users.DTOs;
using AiCallCenter.Application.Users.Interfaces;
using MediatR;

namespace AiCallCenter.Application.Users.Queries.GetUsers;

public sealed class GetUsersQueryHandler
    : IRequestHandler<GetUsersQuery, IReadOnlyList<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersQueryHandler(
        IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<UserDto>> Handle(
        GetUsersQuery request,
        CancellationToken cancellationToken)
    {
        var users =
            await _userRepository.GetAllAsync(
                request.CompanyId,
                cancellationToken);

        return users
            .Select(user => user.ToDto())
            .ToList();
    }
}