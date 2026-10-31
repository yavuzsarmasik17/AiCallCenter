using AiCallCenter.Application.KnowledgeBases.Interfaces;
using MediatR;

namespace AiCallCenter.Application.KnowledgeBases.Commands.DeleteKnowledgeBase;

public sealed class DeleteKnowledgeBaseCommandHandler
    : IRequestHandler<DeleteKnowledgeBaseCommand>
{
    private readonly IKnowledgeBaseRepository
        _knowledgeBaseRepository;

    public DeleteKnowledgeBaseCommandHandler(
        IKnowledgeBaseRepository knowledgeBaseRepository)
    {
        _knowledgeBaseRepository =
            knowledgeBaseRepository;
    }

    public async Task Handle(
        DeleteKnowledgeBaseCommand request,
        CancellationToken cancellationToken)
    {
        var knowledgeBase =
            await _knowledgeBaseRepository.GetByIdAsync(
                request.CompanyId,
                request.KnowledgeBaseId,
                cancellationToken);

        if (knowledgeBase is null)
        {
            throw new KeyNotFoundException(
                "Bilgi bankası bulunamadı.");
        }

        _knowledgeBaseRepository.Delete(
            knowledgeBase);

        await _knowledgeBaseRepository.SaveChangesAsync(
            cancellationToken);
    }
}