using AiCallCenter.Application.AiAgents.DTOs;
using AiCallCenter.Application.AiAgents.Interfaces;
using MediatR;

namespace AiCallCenter.Application.AiAgents.Queries.GetAttachedKnowledgeBases;

public sealed class GetAttachedKnowledgeBasesQueryHandler
    : IRequestHandler<
        GetAttachedKnowledgeBasesQuery,
        IReadOnlyList<AiAgentKnowledgeBaseDto>>
{
    private readonly IAiAgentKnowledgeBaseRepository _repository;

    public GetAttachedKnowledgeBasesQueryHandler(
        IAiAgentKnowledgeBaseRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<AiAgentKnowledgeBaseDto>> Handle(
        GetAttachedKnowledgeBasesQuery request,
        CancellationToken cancellationToken)
    {
        var aiAgentExists = await _repository.AiAgentExistsAsync(
            request.CompanyId,
            request.AiAgentId,
            cancellationToken);

        if (!aiAgentExists)
        {
            throw new KeyNotFoundException(
                "Yapay zekâ ajanı bulunamadı.");
        }

        var connections = await _repository.GetByAiAgentIdAsync(
            request.CompanyId,
            request.AiAgentId,
            cancellationToken);

        return connections
            .Select(connection => new AiAgentKnowledgeBaseDto
            {
                KnowledgeBaseId = connection.KnowledgeBaseId,
                Name = connection.KnowledgeBase.Name,
                Description = connection.KnowledgeBase.Description,
                IsActive = connection.KnowledgeBase.IsActive,
                AttachedAt = connection.CreatedAt
            })
            .ToList();
    }
}