using AiCallCenter.Application.KnowledgeBases.DTOs;
using AiCallCenter.Application.KnowledgeBases.Interfaces;
using MediatR;

namespace AiCallCenter.Application.KnowledgeBases.Queries.GetKnowledgeBases;

public sealed class GetKnowledgeBasesQueryHandler
    : IRequestHandler<
        GetKnowledgeBasesQuery,
        IReadOnlyList<KnowledgeBaseDto>>
{
    private readonly IKnowledgeBaseRepository
        _knowledgeBaseRepository;

    public GetKnowledgeBasesQueryHandler(
        IKnowledgeBaseRepository knowledgeBaseRepository)
    {
        _knowledgeBaseRepository = knowledgeBaseRepository;
    }

    public async Task<IReadOnlyList<KnowledgeBaseDto>> Handle(
        GetKnowledgeBasesQuery request,
        CancellationToken cancellationToken)
    {
        var companyExists =
            await _knowledgeBaseRepository.CompanyExistsAsync(
                request.CompanyId,
                cancellationToken);

        if (!companyExists)
        {
            throw new KeyNotFoundException(
                "Belirtilen firma bulunamadı.");
        }

        var knowledgeBases =
            await _knowledgeBaseRepository.GetByCompanyIdAsync(
                request.CompanyId,
                cancellationToken);

        return knowledgeBases
            .Select(knowledgeBase => new KnowledgeBaseDto
            {
                Id = knowledgeBase.Id,
                CompanyId = knowledgeBase.CompanyId,
                Name = knowledgeBase.Name,
                Description = knowledgeBase.Description,
                IsActive = knowledgeBase.IsActive,
                CreatedAt = knowledgeBase.CreatedAt
            })
            .ToList();
    }
}