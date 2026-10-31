using AiCallCenter.Application.KnowledgeBases.DTOs;
using MediatR;

namespace AiCallCenter.Application.KnowledgeBases.Commands.UpdateKnowledgeBase;

public sealed record UpdateKnowledgeBaseCommand(
    Guid CompanyId,
    Guid KnowledgeBaseId,
    string Name,
    string? Description)
    : IRequest<KnowledgeBaseDto>;