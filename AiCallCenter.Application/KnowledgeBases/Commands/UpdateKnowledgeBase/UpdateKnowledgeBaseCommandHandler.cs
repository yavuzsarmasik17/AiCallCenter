using AiCallCenter.Application.KnowledgeBases.DTOs;
using AiCallCenter.Application.KnowledgeBases.Interfaces;
using MediatR;

namespace AiCallCenter.Application.KnowledgeBases.Commands.UpdateKnowledgeBase;

public sealed class UpdateKnowledgeBaseCommandHandler
    : IRequestHandler<
        UpdateKnowledgeBaseCommand,
        KnowledgeBaseDto>
{
    private readonly IKnowledgeBaseRepository
        _knowledgeBaseRepository;

    public UpdateKnowledgeBaseCommandHandler(
        IKnowledgeBaseRepository knowledgeBaseRepository)
    {
        _knowledgeBaseRepository =
            knowledgeBaseRepository;
    }

    public async Task<KnowledgeBaseDto> Handle(
        UpdateKnowledgeBaseCommand request,
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

        var nameExists =
            await _knowledgeBaseRepository
                .NameExistsForAnotherKnowledgeBaseAsync(
                    request.CompanyId,
                    request.KnowledgeBaseId,
                    request.Name.Trim(),
                    cancellationToken);

        if (nameExists)
        {
            throw new InvalidOperationException(
                "Bu firmada aynı isimde başka bir bilgi bankası bulunuyor.");
        }

        knowledgeBase.Update(
            request.Name,
            request.Description);

        await _knowledgeBaseRepository.SaveChangesAsync(
            cancellationToken);

        return new KnowledgeBaseDto
        {
            Id = knowledgeBase.Id,
            CompanyId = knowledgeBase.CompanyId,
            Name = knowledgeBase.Name,
            Description = knowledgeBase.Description,
            IsActive = knowledgeBase.IsActive,
            CreatedAt = knowledgeBase.CreatedAt
        };
    }
}