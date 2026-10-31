using FluentValidation;

namespace AiCallCenter.Application.KnowledgeBases.Commands.DeleteKnowledgeBase;

public sealed class DeleteKnowledgeBaseCommandValidator
    : AbstractValidator<DeleteKnowledgeBaseCommand>
{
    public DeleteKnowledgeBaseCommandValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty()
            .WithMessage("Firma kimliği zorunludur.");

        RuleFor(x => x.KnowledgeBaseId)
            .NotEmpty()
            .WithMessage("Bilgi bankası kimliği zorunludur.");
    }
}