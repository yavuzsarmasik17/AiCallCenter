using FluentValidation;

namespace AiCallCenter.Application.KnowledgeBases.Commands.CreateKnowledgeBase;

public sealed class CreateKnowledgeBaseCommandValidator
    : AbstractValidator<CreateKnowledgeBaseCommand>
{
    public CreateKnowledgeBaseCommandValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty()
            .WithMessage("Firma kimliği zorunludur.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Bilgi bankası adı zorunludur.")
            .MaximumLength(200)
            .WithMessage(
                "Bilgi bankası adı en fazla 200 karakter olabilir.");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage(
                "Açıklama en fazla 2000 karakter olabilir.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}