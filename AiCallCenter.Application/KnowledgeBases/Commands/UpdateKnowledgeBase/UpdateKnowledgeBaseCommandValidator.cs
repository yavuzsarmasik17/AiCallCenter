using FluentValidation;

namespace AiCallCenter.Application.KnowledgeBases.Commands.UpdateKnowledgeBase;

public sealed class UpdateKnowledgeBaseCommandValidator
    : AbstractValidator<UpdateKnowledgeBaseCommand>
{
    public UpdateKnowledgeBaseCommandValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty()
            .WithMessage("Firma kimliği zorunludur.");

        RuleFor(x => x.KnowledgeBaseId)
            .NotEmpty()
            .WithMessage("Bilgi bankası kimliği zorunludur.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Bilgi bankası adı zorunludur.")
            .MaximumLength(200)
            .WithMessage(
                "Bilgi bankası adı en fazla 200 karakter olabilir.");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage(
                "Açıklama en fazla 1000 karakter olabilir.");
    }
}