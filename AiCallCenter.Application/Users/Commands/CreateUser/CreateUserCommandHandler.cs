using AiCallCenter.Application.Users;
using AiCallCenter.Application.Common.Security;
using AiCallCenter.Application.Users.DTOs;
using AiCallCenter.Application.Users.Interfaces;
using AiCallCenter.Domain.Entities;
using MediatR;

namespace AiCallCenter.Application.Users.Commands.CreateUser;

public sealed class CreateUserCommandHandler
    : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserDto> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        var companyExists =
            await _userRepository.CompanyExistsAsync(
                request.CompanyId,
                cancellationToken);

        if (!companyExists)
        {
            throw new KeyNotFoundException(
                "Belirtilen firma bulunamadı.");
        }

        var normalizedEmail = request.Email
            .Trim()
            .ToLowerInvariant();

        var emailExists =
            await _userRepository.EmailExistsAsync(
                request.CompanyId,
                normalizedEmail,
                null,
                cancellationToken);

        if (emailExists)
        {
            throw new InvalidOperationException(
                "Bu e-posta adresiyle kayıtlı bir kullanıcı bulunuyor.");
        }

        if (string.IsNullOrWhiteSpace(request.Password) ||
            request.Password.Length < 8)
        {
            throw new ArgumentException(
                "Parola en az 8 karakter olmalıdır.");
        }

        var passwordHash =
            _passwordHasher.Hash(request.Password);

        var user = new User(
            request.CompanyId,
            request.FirstName,
            request.LastName,
            normalizedEmail,
            passwordHash,
            request.Role);

        await _userRepository.AddAsync(
            user,
            cancellationToken);

        await _userRepository.SaveChangesAsync(
            cancellationToken);

        return user.ToDto();
    }
}
