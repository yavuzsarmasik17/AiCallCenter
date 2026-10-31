namespace AiCallCenter.Application.Common.Documents;

public interface IDocumentTextExtractor
{
    Task<string> ExtractPdfTextAsync(
        Stream fileStream,
        CancellationToken cancellationToken = default);
}