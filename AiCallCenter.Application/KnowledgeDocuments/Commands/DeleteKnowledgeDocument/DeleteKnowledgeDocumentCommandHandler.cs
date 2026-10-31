using AiCallCenter.Application.KnowledgeDocuments.Interfaces;
using MediatR;

namespace AiCallCenter.Application.KnowledgeDocuments.Commands.DeleteKnowledgeDocument;

public sealed class DeleteKnowledgeDocumentCommandHandler
    : IRequestHandler<DeleteKnowledgeDocumentCommand>
{
    private readonly IKnowledgeDocumentRepository
        _knowledgeDocumentRepository;

    public DeleteKnowledgeDocumentCommandHandler(
        IKnowledgeDocumentRepository knowledgeDocumentRepository)
    {
        _knowledgeDocumentRepository =
            knowledgeDocumentRepository;
    }

    public async Task Handle(
        DeleteKnowledgeDocumentCommand request,
        CancellationToken cancellationToken)
    {
        var document =
            await _knowledgeDocumentRepository.GetByIdAsync(
                request.CompanyId,
                request.KnowledgeBaseId,
                request.DocumentId,
                cancellationToken);

        if (document is null)
        {
            throw new KeyNotFoundException(
                "Bilgi bankası belgesi bulunamadı.");
        }

        _knowledgeDocumentRepository.DeleteDocument(
            document);

        await _knowledgeDocumentRepository.SaveChangesAsync(
            cancellationToken);
    }
}