using AiCallCenter.Application.KnowledgeBases.DTOs;
using MediatR;

namespace AiCallCenter.Application.KnowledgeBases.Commands.CreateKnowledgeBase;

public sealed record CreateKnowledgeBaseCommand(
    Guid CompanyId,
    string Name,
    string? Description)
    : IRequest<KnowledgeBaseDto>;