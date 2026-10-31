using FluentValidation;

namespace AiCallCenter.Application.KnowledgeDocuments.Commands.UploadPdfDocument;

public sealed class UploadPdfDocumentCommandValidator
    : AbstractValidator<UploadPdfDocumentCommand>
{
    private const long MaximumFileSize =
        10 * 1024 * 1024;

    public UploadPdfDocumentCommandValidator()
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
            .WithMessage(
                "Belge adı en fazla 300 karakter olabilir.");

        RuleFor(x => x.OriginalFileName)
            .NotEmpty()
            .WithMessage("Dosya adı zorunludur.");

        RuleFor(x => x.MimeType)
            .Equal("application/pdf")
            .WithMessage("Yalnızca PDF dosyası yüklenebilir.");

        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0)
            .WithMessage("PDF dosyası boş olamaz.")
            .LessThanOrEqualTo(MaximumFileSize)
            .WithMessage(
                "PDF dosyası en fazla 10 MB olabilir.");

        RuleFor(x => x.FileStream)
            .NotNull()
            .WithMessage("PDF dosyası zorunludur.");
    }
}