using FluentValidation;

namespace AiCallCenter.Application.KnowledgeDocuments.Commands.CreateTextDocument;

public sealed class CreateTextDocumentCommandValidator
    : AbstractValidator<CreateTextDocumentCommand>
{
    public CreateTextDocumentCommandValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty()
            .WithMessage("Firma kimliği zorunludur.");

        RuleFor(x => x.KnowledgeBaseId)
            .NotEmpty()
            .WithMessage("Bilgi bankası kimliği zorunludur.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Belge adı zorunludur.")
            .MaximumLength(300)
            .WithMessage("Belge adı en fazla 300 karakter olabilir.");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Belge içeriği zorunludur.")
            .MaximumLength(100000)
            .WithMessage("Metin içeriği en fazla 100000 karakter olabilir.");
    }
}