using FluentValidation;

namespace AiCallCenter.Application.KnowledgeDocuments.Commands.DeleteKnowledgeDocument;

public sealed class DeleteKnowledgeDocumentCommandValidator
    : AbstractValidator<DeleteKnowledgeDocumentCommand>
{
    public DeleteKnowledgeDocumentCommandValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty()
            .WithMessage("Firma kimliği zorunludur.");

        RuleFor(x => x.KnowledgeBaseId)
            .NotEmpty()
            .WithMessage("Bilgi bankası kimliği zorunludur.");

        RuleFor(x => x.DocumentId)
            .NotEmpty()
            .WithMessage("Belge kimliği zorunludur.");
    }
}