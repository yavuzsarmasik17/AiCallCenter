using FluentValidation;

namespace AiCallCenter.Application.Companies.Commands.CreateCompany;

public sealed class CreateCompanyCommandValidator
    : AbstractValidator<CreateCompanyCommand>
{
    public CreateCompanyCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Firma adı zorunludur.")
            .MaximumLength(200)
            .WithMessage("Firma adı en fazla 200 karakter olabilir.");

        RuleFor(x => x.Slug)
            .NotEmpty()
            .WithMessage("Firma kısa adı zorunludur.")
            .MaximumLength(100)
            .WithMessage("Firma kısa adı en fazla 100 karakter olabilir.")
            .Matches("^[a-zA-Z0-9ğüşöçıİĞÜŞÖÇ _-]+$")
            .WithMessage("Firma kısa adı geçersiz karakter içeriyor.");

        RuleFor(x => x.TaxNumber)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.TaxNumber));

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(30)
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.Email)
            .EmailAddress()
            .WithMessage("Geçerli bir e-posta adresi giriniz.")
            .MaximumLength(320)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.SubscriptionPlan)
            .IsInEnum()
            .WithMessage("Abonelik paketi geçersiz.");
    }
}