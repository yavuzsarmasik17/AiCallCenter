using FluentValidation;

namespace AiCallCenter.Application.Users.Commands.CreateUser;

public sealed class CreateUserCommandValidator
    : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty()
            .WithMessage("Firma kimliği zorunludur.");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("Kullanıcı adı zorunludur.")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Kullanıcı soyadı zorunludur.")
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("E-posta adresi zorunludur.")
            .EmailAddress()
            .WithMessage("Geçerli bir e-posta adresi giriniz.")
            .MaximumLength(320);

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Parola zorunludur.")
            .MinimumLength(8)
            .WithMessage("Parola en az 8 karakter olmalıdır.")
            .MaximumLength(100)
            .WithMessage("Parola en fazla 100 karakter olabilir.");

        RuleFor(x => x.Password)
            .Matches("[A-Z]")
            .WithMessage("Parola en az bir büyük harf içermelidir.");

        RuleFor(x => x.Password)
            .Matches("[a-z]")
            .WithMessage("Parola en az bir küçük harf içermelidir.");

        RuleFor(x => x.Password)
            .Matches("[0-9]")
            .WithMessage("Parola en az bir rakam içermelidir.");

        RuleFor(x => x.Role)
            .IsInEnum()
            .WithMessage("Kullanıcı rolü geçersiz.");
    }
}