
using AiCallCenter.Application.Authentication.DTOs;
using AiCallCenter.Application.Common.Security;
using AiCallCenter.Application.Users.Interfaces;
using MediatR;
using Microsoft.Extensions.Options;

namespace AiCallCenter.Application.Authentication.Commands.Login;

public sealed class LoginCommandHandler
    : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly JwtExpirationOptions _expirationOptions;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IOptions<JwtExpirationOptions> expirationOptions)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _expirationOptions = expirationOptions.Value;
    }

    public async Task<LoginResponse> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(
            request.Email,
            cancellationToken);

        if (user is null ||
            !_passwordHasher.Verify(
                request.Password,
                user.PasswordHash))
        {
            throw new UnauthorizedAccessException(
                "E-posta adresi veya parola hatalı.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException(
                "Kullanıcı hesabı aktif değil.");
        }

        user.MarkLogin();

        await _userRepository.SaveChangesAsync(
            cancellationToken);

        var token = _jwtTokenGenerator.GenerateToken(user);

        return new LoginResponse
        {
            AccessToken = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(
                _expirationOptions.ExpirationMinutes),
            UserId = user.Id,
            CompanyId = user.CompanyId,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role
        };
    }
}