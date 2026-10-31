using System.Text;
using AiCallCenter.Application.Common.Documents;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace AiCallCenter.Infrastructure.Documents;

public sealed class PdfTextExtractor : IDocumentTextExtractor
{
    public Task<string> ExtractPdfTextAsync(
        Stream fileStream,
        CancellationToken cancellationToken = default)
    {
        if (fileStream is null)
        {
            throw new ArgumentNullException(nameof(fileStream));
        }

        if (!fileStream.CanRead)
        {
            throw new InvalidOperationException(
                "PDF dosya akışı okunamıyor.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        using var memoryStream = new MemoryStream();

        fileStream.CopyTo(memoryStream);

        memoryStream.Position = 0;

        using var document = PdfDocument.Open(memoryStream);

        var textBuilder = new StringBuilder();

        foreach (var page in document.GetPages())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var pageText =
                ContentOrderTextExtractor.GetText(page);

            if (string.IsNullOrWhiteSpace(pageText))
            {
                continue;
            }

            textBuilder.AppendLine(pageText.Trim());
            textBuilder.AppendLine();
        }

        var extractedText = textBuilder
            .ToString()
            .Trim();

        if (string.IsNullOrWhiteSpace(extractedText))
        {
            throw new InvalidOperationException(
                "PDF içerisinden okunabilir metin çıkarılamadı.");
        }

        return Task.FromResult(extractedText);
    }
}