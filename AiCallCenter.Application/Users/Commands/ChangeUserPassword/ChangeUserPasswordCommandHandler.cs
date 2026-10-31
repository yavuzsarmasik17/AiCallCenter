using AiCallCenter.Application.Common.Security;
using AiCallCenter.Application.Users.Interfaces;
using MediatR;

namespace AiCallCenter.Application.Users.Commands.ChangeUserPassword;

public sealed class ChangeUserPasswordCommandHandler
    : IRequestHandler<ChangeUserPasswordCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ChangeUserPasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task Handle(
        ChangeUserPasswordCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.NewPassword) ||
            request.NewPassword.Length < 8)
        {
            throw new ArgumentException(
                "Yeni parola en az 8 karakter olmalıdır.");
        }

        var user = await _userRepository.GetByIdAsync(
            request.CompanyId,
            request.UserId,
            cancellationToken);

        if (user is null)
            throw new KeyNotFoundException("Kullanıcı bulunamadı.");

        user.ChangePassword(
            _passwordHasher.Hash(request.NewPassword));

        await _userRepository.SaveChangesAsync(cancellationToken);
    }
}
